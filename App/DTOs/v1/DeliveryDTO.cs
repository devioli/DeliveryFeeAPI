using System.ComponentModel.DataAnnotations;

namespace App.DTOs.v1;

/// <summary>
/// Data transfer object for delivery fee calculation request parameters.
/// </summary>
public class DeliveryDTO
{
    /// <summary>
    /// The city where the delivery takes place.
    /// </summary>
    /// <example>tallinn</example>
    [Required(ErrorMessage = "City is required")]
    public required string City { get; set; }

    /// <summary>
    /// The type of vehicle used for delivery (car, scooter, bike).
    /// </summary>
    /// <example>car</example>
    [Required(ErrorMessage = "Vehicle type is required")]
    public required string VehicleType { get; set; }

    /// <summary>
    /// Optional Unix timestamp to calculate fees for a specific point in time.
    /// If not provided, the current time will be used.
    /// </summary>
    /// <example>1618560000</example>
    public long? Timestamp { get; set; }
}