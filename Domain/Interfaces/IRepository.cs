using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces;

/// <summary>
/// Defines the contract for repository operations
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Gets all the necessary data for calculating delivery fee in a single query
    /// </summary>
    Task<DeliveryFeeDataDto> GetDeliveryFeeDataAsync(string city, string vehicleType, DateTime? dateTime);
    
    /// <summary>
    /// Gets the regional base fee for a specific station, vehicle, and fee type
    /// </summary>
    Task<double> GetRegionalBaseFeeAsync(Guid stationId, Guid vehicleId, Guid feeTypeId);
    
    /// <summary>
    /// Gets the vehicle ID by its name
    /// </summary>
    Task<Guid?> GetVehicleIdByNameAsync(string vehicle);
    
    /// <summary>
    /// Gets the fee type ID by its code
    /// </summary>
    Task<Guid?> GetFeeTypeIdByCodeAsync(string code);
    
    /// <summary>
    /// Gets the latest weather forecast for a specific station
    /// </summary>
    Task<WeatherForecastDto?> GetLatestForecastByStationAsync(Guid stationId);
    
    /// <summary>
    /// Gets a weather forecast for a specific station and time
    /// </summary>
    Task<WeatherForecastDto?> GetForecastByTimeAndStationAsync(Guid stationId, DateTime date);
    
    /// <summary>
    /// Gets all weather stations
    /// </summary>
    Task<IEnumerable<WeatherStationDto>> GetAllWeatherStationsAsync();
    
    /// <summary>
    /// Gets all weather conditions
    /// </summary>
    Task<IEnumerable<WeatherConditionDto>> GetAllWeatherConditionsAsync();
    
    /// <summary>
    /// Gets a weather station ID by city name
    /// </summary>
    Task<Guid?> GetWeatherStationIdByCityAsync(string city);
} 