using App.DTOs.v1;
using Domain.Interfaces;
using Domain.Models;

namespace App.Endpoints.v1;

/// <summary>
/// Contains endpoint definitions for delivery fee calculation API.
/// </summary>
public static class DeliveryEndpoints
{
    /// <summary>
    /// Maps all delivery-related endpoints to the application's route builder.
    /// </summary>
    /// <param name="routes">The endpoint route builder to add routes to.</param>
    public static void MapDeliveryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("api/v1/delivery")
            .WithTags("Delivery")
            .WithDescription("Calculates delivery fee for food couriers based on regional base fee, vehicle type and weather conditions.")
            .WithOpenApi()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesValidationProblem();

        group.MapGet("{city}/{vehicleType}",
                async (string city, string vehicleType, long? timestamp, IService service) =>
                {
                    var delivery = CreateDeliveryRequest(city, vehicleType, timestamp);
                    var result = await service.GetDeliveryFeeAsync(delivery);
                    return TypedResults.Ok(new DeliveryDtoResponse {Fee = result});
                })
            .WithName("GetDeliveryFee")
            .Produces<DeliveryDtoResponse>();

        group.MapGet("", async ([AsParameters] DeliveryDto dto, IService service) =>
            {
                var delivery = CreateDeliveryRequest(dto.City, dto.VehicleType, dto.Timestamp);
                var result = await service.GetDeliveryFeeAsync(delivery);
                return TypedResults.Ok(new DeliveryDtoResponse {Fee = result});
            })
            .WithName("GetDeliveryFeeFromQuery")
            .Produces<DeliveryDtoResponse>();
    }
    
    private static Delivery CreateDeliveryRequest(string city, string vehicleType, long? timestamp)
    {
        return new Delivery
        {
            City = city.ToLower(),
            VehicleType = vehicleType.ToLower(),
            DateTime = timestamp.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).DateTime
                : null
        };
    }
} 