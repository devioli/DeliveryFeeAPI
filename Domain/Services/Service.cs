using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Services;

public class Service(IRepository repository) : IService
{
    public async Task<double> GetDeliveryFeeAsync(DeliveryFee deliveryFee)
    {
        try
        {
            var data = await repository.GetDeliveryFeeDataAsync(deliveryFee.City, deliveryFee.VehicleType, deliveryFee.DateTime);
            
            if (!data.StationId.HasValue)
                throw new NotFoundException($"Weather station for city '{deliveryFee.City}' was not found.");
                
            if (!data.VehicleId.HasValue)
                throw new NotFoundException($"Vehicle type with name '{deliveryFee.VehicleType}' was not found.");
                
            if (!data.FeeTypeId.HasValue)
                throw new NotFoundException("Regional base fee type was not found.");
                
            if (data.WeatherForecast == null)
            {
                var errorMessage = deliveryFee.DateTime.HasValue
                    ? $"Weather forecast data from station '{data.StationId.Value}' with timestamp '{new DateTimeOffset(deliveryFee.DateTime.Value).ToUnixTimeSeconds()}' was not found."
                    : $"Weather forecast data from station '{data.StationId.Value}' was not found.";
                    
                throw new NotFoundException(errorMessage);
            }

            var airTemperatureFee = GetAirTemperatureFee(data.WeatherForecast.AirTemperature, deliveryFee.VehicleType);
            var windSpeedFee = GetWindSpeedFee(data.WeatherForecast.WindSpeed, deliveryFee.VehicleType);
            var weatherConditionFee = GetConditionFee(data.WeatherForecast.Phenomenon!, deliveryFee.VehicleType, data.WeatherConditions);
            
            return data.RegionalBaseFee + airTemperatureFee + windSpeedFee + weatherConditionFee;
        }
        catch (Exception ex) when (ex is not (NotFoundException or ForbiddenVehicleTypeException))
        {
            var stationId = await repository.GetWeatherStationIdByCityAsync(deliveryFee.City);
            if (!stationId.HasValue)
                throw new NotFoundException($"Weather station for city '{deliveryFee.City}' was not found.");

            var vehicleId = await repository.GetVehicleIdByNameAsync(deliveryFee.VehicleType);
            if (!vehicleId.HasValue)
                throw new NotFoundException($"Vehicle type with name '{deliveryFee.VehicleType}' was not found.");
                
            var feeTypeId = await repository.GetFeeTypeIdByCodeAsync("rbf");
            if (!feeTypeId.HasValue)
                throw new NotFoundException("Regional base fee type was not found.");
                
            var regionalBaseFee = await repository.GetRegionalBaseFeeAsync(stationId.Value, vehicleId.Value, feeTypeId.Value);

            var weatherForecast = deliveryFee.DateTime.HasValue
                ? await GetForecastByTimeAndStationAsync(stationId.Value, deliveryFee.DateTime.Value)
                : await GetLatestForecastByStationAsync(stationId.Value);

            var airTemperatureFee = GetAirTemperatureFee(weatherForecast.AirTemperature, deliveryFee.VehicleType);
            var windSpeedFee = GetWindSpeedFee(weatherForecast.WindSpeed, deliveryFee.VehicleType);
            var weatherConditionFee = await GetConditionFee(weatherForecast.Phenomenon!, deliveryFee.VehicleType);
            
            return regionalBaseFee + airTemperatureFee + windSpeedFee + weatherConditionFee;
        }
    }

    public async Task<WeatherForecastDto> GetLatestForecastByStationAsync(Guid stationId)
    {
        var weatherForecast = await repository.GetLatestForecastByStationAsync(stationId);
        if (weatherForecast is null) throw new NotFoundException($"Weather forecast data from station '{stationId}' was not found.");
        return weatherForecast;
    }
    
    public async Task<WeatherForecastDto> GetForecastByTimeAndStationAsync(Guid stationId, DateTime date)
    {
        var weatherForecast = await repository.GetForecastByTimeAndStationAsync(stationId, date);
        if (weatherForecast is not null) return weatherForecast;
        var timestamp = new DateTimeOffset(date).ToUnixTimeSeconds();
        throw new NotFoundException($"Weather forecast data from station '{stationId}' with timestamp '{timestamp}' was not found.");
    }
    
    public double GetAirTemperatureFee(double temperature, string vehicle)
    {
        if (vehicle is not ("scooter" or "bike")) return 0;
        return temperature switch
        {
            < -10 => 1,
            < 0 and > -10 => 0.5,
            _ => 0
        };
    }
    
    public double GetWindSpeedFee(double windSpeed, string vehicle)
    {
        if (vehicle is not "bike") return 0;
        return windSpeed switch
        {
            > 20 => throw new ForbiddenVehicleTypeException("Usage of selected vehicle type is forbidden."),
            > 10 and < 20 => 0.5,
            _ => 0
        };
    }
    
    public double GetConditionFee(string condition, string vehicle, IEnumerable<WeatherConditionDto> weatherConditions)
    {
        if (condition is "" or null) return 0;
        if (vehicle is not ("scooter" or "bike")) return 0;
        
        var conditionLowerCase = condition.ToLower();
        
        var conditionDict = weatherConditions.ToDictionary(
            g => g.Grade,
            g => g.Conditions.Select(x => x.Name.ToLower()).ToList()
        );
        
        conditionDict.TryGetValue(3, out var forbiddenConditions);
        conditionDict.TryGetValue(2, out var snowyConditions);
        conditionDict.TryGetValue(1, out var rainyConditions);

        if (forbiddenConditions?.FirstOrDefault(term => conditionLowerCase.Contains(term)) != null)
        {
            throw new ForbiddenVehicleTypeException("Usage of selected vehicle type is forbidden.");
        }

        if (snowyConditions?.FirstOrDefault(term => conditionLowerCase.Contains(term)) != null)
        {
            return 1.0;
        }

        if (rainyConditions?.FirstOrDefault(term => conditionLowerCase.Contains(term)) != null)
        {
            return 0.5;
        }
        
        return 0;
    }
    
    public async Task<double> GetConditionFee(string condition, string vehicle)
    {
        if (condition is "" or null) return 0;
        if (vehicle is not ("scooter" or "bike")) return 0;
        
        var weatherConditions = await repository.GetAllWeatherConditionsAsync();
        return GetConditionFee(condition, vehicle, weatherConditions);
    }
}