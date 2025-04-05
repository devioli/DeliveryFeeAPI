namespace App.DTOs.v1;

/// <summary>
/// Data transfer object for the delivery fee calculation response.
/// </summary>
public class DeliveryDtoResponse
{
    /// <summary>
    /// The calculated delivery fee in euros.
    /// </summary>
    /// <example>4.5</example>
    public double Fee { get; set; }
}