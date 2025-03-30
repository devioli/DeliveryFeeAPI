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
    Task<DeliveryFeeContext> GetDeliveryFeeContextAsync(string city, string vehicleType, DateTime? dateTime);
    
    /// <summary>
    /// Gets all weather stations
    /// </summary>
    Task<IEnumerable<StationDto>> GetAllWeatherStationsAsync();
} 