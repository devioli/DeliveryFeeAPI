using Asp.Versioning;
using App.DTOs.v1;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers.api.v1;

/// <summary>
/// API controller responsible for calculating delivery fees based on location, vehicle type and weather conditions.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class DeliveryController : ControllerBase
{
    private readonly IService _service;

    public DeliveryController(IService service)
    {
        _service = service;
    }

    /// <summary>
    /// Calculates delivery fee for food couriers based on regional base fee, vehicle type and weather conditions.
    /// </summary>
    /// <param name="city">The city where the delivery takes place (case-insensitive)</param>
    /// <param name="vehicleType">The type of vehicle used for delivery (car, scooter, bike; case-insensitive)</param>
    /// <param name="timestamp">Optional Unix timestamp to calculate fees for a specific point in time</param>
    [HttpGet("{city}/{vehicleType}")]
    [ProducesResponseType(typeof(DeliveryFeeDTOResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeliveryFeeAsync(string city, string vehicleType, [FromQuery] long? timestamp)
    {
        return await CalculateDeliveryFee(city, vehicleType, timestamp);
    }
    
    /// <summary>
    /// Calculates delivery fee for food couriers using query parameters.
    /// </summary>
    /// <param name="dto">The DTO containing city, vehicle type and optional timestamp</param>
    [HttpGet]
    [ProducesResponseType(typeof(DeliveryFeeDTOResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeliveryFeeFromQueryAsync([FromQuery] DeliveryFeeDTO dto)
    {
        return await CalculateDeliveryFee(dto.City, dto.VehicleType, dto.Timestamp);
    }

    private async Task<IActionResult> CalculateDeliveryFee(string city, string vehicleType, long? timestamp)
    {
        var result = await _service.GetDeliveryFeeAsync(
            new DeliveryFee
            {
                City = city.ToLower(), 
                VehicleType = vehicleType.ToLower(),
                DateTime = timestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).DateTime : null
            });
        return Ok(new DeliveryFeeDTOResponse { DeliveryFee = result });
    }
} 