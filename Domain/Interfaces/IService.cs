using Domain.Models;

namespace Domain.Interfaces;

/// <summary>
/// Defines the contract for the delivery fee calculation service
/// </summary>
public interface IService
{
    /// <summary>
    /// Calculates the delivery fee based on vehicle type, city, and optional datetime
    /// </summary>
    /// <param name="delivery">The delivery fee request containing vehicle type, city, and optional datetime</param>
    /// <returns>The calculated delivery fee including base fee and any additional weather-related fees</returns>
    Task<double> GetDeliveryFeeAsync(Delivery delivery);
} 