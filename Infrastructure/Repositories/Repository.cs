using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Hybrid;
using static Domain.Constants.Constants;

namespace Infrastructure.Repositories;

public class Repository(AppDbContext dbContext, HybridCache hybridCache) : IRepository
{
    public async Task<DeliveryFeeContext> GetDeliveryFeeContextAsync(string city, string vehicleType, DateTime? dateTime)
    {
        var baseData = await hybridCache.GetOrCreateAsync(
            $"base-{city}-{vehicleType}",
            async token => await GetBaseDataAsync(city, vehicleType),
            tags: ["baseData"]
        );
        
        if (baseData is null)
        {
            var stationQuery = await dbContext.Locations
                .Where(l => l.Name == city)
                .Select(l => l.WeatherStationId)
                .FirstOrDefaultAsync();
                
            var vehicleQuery = await dbContext.VehicleTypes
                .Where(v => v.Name == vehicleType)
                .Select(v => v.Id)
                .FirstOrDefaultAsync();
                
            var feeTypeQuery = await dbContext.FeeTypes
                .Where(f => f.Code == Fees.Rbf)
                .Select(f => f.Id)
                .FirstOrDefaultAsync();
                
            return new DeliveryFeeContext
            {
                StationId = stationQuery == Guid.Empty ? null : stationQuery,
                VehicleId = vehicleQuery == Guid.Empty ? null : vehicleQuery,
                FeeTypeId = feeTypeQuery == Guid.Empty ? null : feeTypeQuery
            };
        }
        
        var forecast = await hybridCache.GetOrCreateAsync(
            $"weather-{baseData.StationId}-{dateTime?.ToString("yyyy-MM-dd-HH:mm") ?? "current"}",
            async token => await GetWeatherForecastAsync(baseData.StationId, dateTime),
            tags: ["weather"]
        );
        
        if (forecast?.Phenomenon is null)
        {
            return new DeliveryFeeContext
            {
                StationId = baseData.StationId,
                VehicleId = baseData.VehicleId,
                FeeTypeId = baseData.FeeTypeId,
                RegionalBaseFee = baseData.RegionalBaseFee
            };
        }
        
        var grade = await hybridCache.GetOrCreateAsync(
            $"phenomenon-{forecast.Phenomenon}", 
            async token => await dbContext.WeatherConditions
            .Where(wc => 
                forecast.Phenomenon != null && 
                forecast.Phenomenon.Contains(wc.Name))
            .Join(
                dbContext.ConditionTypes,
                wc => wc.ConditionTypeId,
                ct => ct.Id,
                (wc, ct) => ct
            )
            .Select(x => x.Grade)
                .FirstOrDefaultAsync(cancellationToken: token),
            tags: ["phenomenon"]
        );
            
        return new DeliveryFeeContext
        {
            StationId = baseData.StationId,
            VehicleId = baseData.VehicleId,
            FeeTypeId = baseData.FeeTypeId,
            RegionalBaseFee = baseData.RegionalBaseFee,
            WeatherConditionGrade = grade,
            WeatherForecast = MapToForecastDto(forecast),
        };
    }  
    
    private async Task<BaseData?> GetBaseDataAsync(string city, string vehicleType)
    {
        const string sql = 
            """
            SELECT
                station.Id AS StationId,
                vehicle.Id AS VehicleId,
                feeType.Id AS FeeTypeId,
                fee.Amount AS RegionalBaseFee
            FROM Locations AS location
            JOIN WeatherStations AS station ON location.WeatherStationId = station.Id
            JOIN VehicleTypes AS vehicle ON 1=1
            JOIN FeeTypes AS feeType ON 1=1
            JOIN Fees AS fee ON fee.WeatherStationId = station.Id
                            AND fee.VehicleTypeId = vehicle.Id
                            AND fee.FeeTypeId = feeType.Id
            WHERE location.Name = @city
              AND vehicle.Name = @vehicleType
              AND feeType.Code = @feeTypeCode
            """;

        var parameters = new[]
        {
            new SqliteParameter("@city", city),
            new SqliteParameter("@vehicleType", vehicleType),
            new SqliteParameter("@feeTypeCode", Fees.Rbf)
        };

        // Execute the raw SQL query
        return await dbContext.Database
            .SqlQueryRaw<BaseData>(sql, parameters)
            .FirstOrDefaultAsync();
    }
    
    private async Task<WeatherForecast?> GetWeatherForecastAsync(Guid stationId, DateTime? dateTime)
    {
        WeatherForecast? forecast;
        if (dateTime.HasValue)
        {
            const string sql = 
                """
                SELECT * 
                FROM WeatherForecasts 
                WHERE DateTime >= datetime(@date, 'start of day') 
                    AND DateTime < datetime(@date, 'start of day', '+1 day') 
                    AND WeatherStationId = @stationId 
                ORDER BY ABS(unixepoch(DateTime) - unixepoch(@dateTime))
                LIMIT 1
                """;
            
            var parameters = new[]
            {
                new SqliteParameter("@date", dateTime.Value.Date),
                new SqliteParameter("@stationId", stationId),
                new SqliteParameter("@dateTime", dateTime)
            };
            
            forecast = await dbContext.WeatherForecasts
            .FromSqlRaw(sql, parameters)
            .FirstOrDefaultAsync();
        }
        else
        {
            forecast = await dbContext.WeatherForecasts
                .Where(x => x.DateTime.Date == DateTime.Today)
                .Where(y => y.WeatherStationId == stationId)
                .OrderByDescending(x => x.DateTime)
                .FirstOrDefaultAsync();
        }
        return forecast;
    }

    public async Task<IEnumerable<StationDto>> GetAllWeatherStationsAsync()
    {
        var stations = await dbContext.WeatherStations.ToListAsync();
        return stations.Select(MapToStationDto);
    }
    
    public ForecastDto MapToForecastDto(WeatherForecast forecast)
    {
        return new ForecastDto
        {
            Id = forecast.Id,
            AirTemperature = forecast.AirTemperature,
            WindSpeed = forecast.WindSpeed,
            Phenomenon = forecast.Phenomenon,
            DateTime = forecast.DateTime,
            WeatherStationId = forecast.WeatherStationId
        };
    }

    public StationDto MapToStationDto(WeatherStation station)
    {
        return new StationDto
        {
            Id = station.Id,
            Name = station.Name,
            WmoCode = station.WmoCode
        };
    }
}

/// <summary>
/// Class to hold the base delivery fee data from SQL query
/// </summary>
public class BaseData
{
    public Guid StationId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid FeeTypeId { get; set; }
    public double RegionalBaseFee { get; set; }
}