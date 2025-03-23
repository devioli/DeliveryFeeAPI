using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models.Weather.Station;
using Infrastructure.Persistence.Models.Weather.Condition;
using Infrastructure.Persistence.Models.Weather.Forecast;
using Domain.Interfaces;
using Domain.Models;
using System.Linq;

namespace Infrastructure.Repositories;

public class Repository(AppDbContext dbContext) : IRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Gets all the necessary data for calculating delivery fee in a single query
    /// </summary>
    public async Task<DeliveryFeeDataDto> GetDeliveryFeeDataAsync(string city, string vehicleType, DateTime? dateTime)
    {
        var sql = @"
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
              AND feeType.Code = 'rbf'";

        // Execute the raw SQL query
        var baseData = await _dbContext.Database.SqlQueryRaw<DeliveryFeeBaseData>(
                sql,
                new Microsoft.Data.Sqlite.SqliteParameter("@city", city),
                new Microsoft.Data.Sqlite.SqliteParameter("@vehicleType", vehicleType))
            .FirstOrDefaultAsync();
        
        if (baseData == null)
        {
            // If data is missing, do targeted queries to identify what's missing
            var stationQuery = await _dbContext.Locations
                .Where(l => l.Name == city)
                .Select(l => l.WeatherStationId)
                .FirstOrDefaultAsync();
                
            var vehicleQuery = await _dbContext.VehicleTypes
                .Where(v => v.Name == vehicleType)
                .Select(v => v.Id)
                .FirstOrDefaultAsync();
                
            var feeTypeQuery = await _dbContext.FeeTypes
                .Where(f => f.Code == "rbf")
                .Select(f => f.Id)
                .FirstOrDefaultAsync();
                
            return new DeliveryFeeDataDto
            {
                StationId = stationQuery == Guid.Empty ? null : stationQuery,
                VehicleId = vehicleQuery == Guid.Empty ? null : vehicleQuery,
                FeeTypeId = feeTypeQuery == Guid.Empty ? null : feeTypeQuery
            };
        }
        
        // Get the weather forecast data
        WeatherForecast? forecast;
        if (dateTime.HasValue)
        {
            forecast = await _dbContext.WeatherForecasts
                .FromSqlRaw(
                    "SELECT * " +
                    "FROM WeatherForecasts " +
                    "WHERE DateTime >= datetime({0}, 'start of day') " +
                    "AND DateTime < datetime({0}, 'start of day', '+1 day') " +
                    "AND WeatherStationId = {1} " +
                    "ORDER BY ABS(unixepoch(DateTime) - unixepoch({2})) " +
                    "LIMIT 1",
                    dateTime.Value.Date, 
                    baseData.StationId, 
                    dateTime.Value)
                .FirstOrDefaultAsync();
        }
        else
        {
            forecast = await _dbContext.WeatherForecasts
                .Where(x => x.DateTime.Date == DateTime.Today)
                .Where(y => y.WeatherStationId == baseData.StationId)
                .OrderByDescending(x => x.DateTime)
                .FirstOrDefaultAsync();
        }
        
        // Get all weather conditions in a single query with eager loading
        var conditions = await _dbContext.ConditionTypes
            .Include(c => c.Conditions)
            .AsNoTracking()
            .ToListAsync();
            
        return new DeliveryFeeDataDto
        {
            StationId = baseData.StationId,
            VehicleId = baseData.VehicleId,
            FeeTypeId = baseData.FeeTypeId,
            RegionalBaseFee = baseData.RegionalBaseFee,
            WeatherForecast = forecast != null ? MapToWeatherForecastDto(forecast) : null,
            WeatherConditions = conditions.Select(MapToWeatherConditionDto).ToList()
        };
    }
    
    public async Task<double> GetRegionalBaseFeeAsync(Guid stationId, Guid vehicleId, Guid feeTypeId)
    {
        return await _dbContext.Fees
            .Where(x => x.FeeTypeId == feeTypeId)
            .Where(v => v.VehicleTypeId == vehicleId)
            .Where(l => l.WeatherStationId == stationId)
            .Select(a => a.Amount)
            .FirstOrDefaultAsync(); 
    }
    
    public async Task<Guid?> GetVehicleIdByNameAsync(string vehicle)
    {
        var id = await _dbContext.VehicleTypes
            .Where(x => x.Name == vehicle)
            .Select(y => y.Id)
            .FirstOrDefaultAsync();

        return id == Guid.Empty ? null : id;
    }
    
    public async Task<Guid?> GetFeeTypeIdByCodeAsync(string code)
    {
        var id = await _dbContext.FeeTypes
             .Where(x => x.Code == code)
             .Select(x => x.Id)
             .FirstOrDefaultAsync();
        
        return id == Guid.Empty ? null : id;
    }

    public async Task<WeatherForecastDto?> GetLatestForecastByStationAsync(Guid stationId)
    {
        var forecast = await _dbContext.WeatherForecasts
            .Where(x => x.DateTime.Date == DateTime.Today)
            .Where(y => y.WeatherStationId == stationId)
            .OrderByDescending(x => x.DateTime)
            .FirstOrDefaultAsync();

        return forecast != null ? MapToWeatherForecastDto(forecast) : null;
    }
    
    public async Task<WeatherForecastDto?> GetForecastByTimeAndStationAsync(Guid stationId, DateTime date)
    {
        var forecast = await _dbContext.WeatherForecasts
            .FromSqlRaw(
                "SELECT * " +
                "FROM WeatherForecasts " +
                "WHERE DateTime >= datetime({0}, 'start of day') " +
                "AND DateTime < datetime({0}, 'start of day', '+1 day') " +
                "AND WeatherStationId = {1} " +
                "ORDER BY ABS(unixepoch(DateTime) - unixepoch({2})) " +
                "LIMIT 1",
                date.Date, 
                stationId, 
                date)
            .FirstOrDefaultAsync();

        return forecast != null ? MapToWeatherForecastDto(forecast) : null;
    }

    public async Task<IEnumerable<WeatherStationDto>> GetAllWeatherStationsAsync()
    {
        var stations = await _dbContext.WeatherStations.ToListAsync();
        return stations.Select(MapToWeatherStationDto);
    }
    
    public async Task<IEnumerable<WeatherConditionDto>> GetAllWeatherConditionsAsync()
    {
        var conditions = await _dbContext.ConditionTypes
            .Include(c => c.Conditions)
            .ToListAsync();
        
        return conditions.Select(MapToWeatherConditionDto);
    }
    
    public async Task<Guid?> GetWeatherStationIdByCityAsync(string city)
    {
        var id = await _dbContext.Locations
            .Where(x => x.Name == city)
            .Select(x => x.WeatherStationId)
            .FirstOrDefaultAsync();

        return id == Guid.Empty ? null : id;
    }

    #region Mapping Methods

    private static WeatherForecastDto MapToWeatherForecastDto(WeatherForecast forecast)
    {
        return new WeatherForecastDto
        {
            Id = forecast.Id,
            AirTemperature = forecast.AirTemperature,
            WindSpeed = forecast.WindSpeed,
            Phenomenon = forecast.Phenomenon,
            DateTime = forecast.DateTime,
            WeatherStationId = forecast.WeatherStationId
        };
    }

    private static WeatherStationDto MapToWeatherStationDto(WeatherStation station)
    {
        return new WeatherStationDto
        {
            Id = station.Id,
            Name = station.Name,
            WmoCode = station.WmoCode
        };
    }

    private static WeatherConditionDto MapToWeatherConditionDto(ConditionType conditionType)
    {
        return new WeatherConditionDto
        {
            Id = conditionType.Id,
            Grade = conditionType.Grade,
            Conditions = conditionType.Conditions?.Select(c => new ConditionDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList() ?? new List<ConditionDto>()
        };
    }

    #endregion
}

/// <summary>
/// Class to hold the base delivery fee data from SQL query
/// </summary>
public class DeliveryFeeBaseData
{
    public Guid StationId { get; set; }
    public Guid VehicleId { get; set; }
    public Guid FeeTypeId { get; set; }
    public double RegionalBaseFee { get; set; }
}