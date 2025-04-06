using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using static Domain.Constants.Constants;

namespace Domain.Services;

public class Service(IRepository repository) : IService
{
    public async Task<double> GetDeliveryFeeAsync(Delivery delivery)
    {
        ValidateDelivery(delivery);
        var data = await repository.GetDeliveryFeeContextAsync(delivery.City, delivery.VehicleType, delivery.DateTime);
        ValidateDeliveryFeeContext(data, delivery);

        var airTemperatureFee = GetAirTemperatureFee(data.WeatherForecast!.AirTemperature, delivery.VehicleType);
        var windSpeedFee = GetWindSpeedFee(data.WeatherForecast.WindSpeed, delivery.VehicleType);
        var weatherConditionFee = GetConditionFee(data.WeatherConditionGrade, delivery.VehicleType);
        return data.RegionalBaseFee + airTemperatureFee + windSpeedFee + weatherConditionFee;
    }
    
    public double GetAirTemperatureFee(double temperature, string vehicle)
    {
        if (vehicle is not (Vehicles.Bike or Vehicles.Scooter)) return 0;
        return temperature switch
        {
            < -10 => 1,
            < 0 and > -10 => 0.5,
            _ => 0
        };
    }
    
    public double GetWindSpeedFee(double windSpeed, string vehicle)
    {
        if (vehicle is not Vehicles.Bike) return 0;
        return windSpeed switch
        {
            > 20 => throw new ForbiddenVehicleTypeException("Usage of selected vehicle type is forbidden."),
            > 10 and < 20 => 0.5,
            _ => 0
        };
    }
    
    public double GetConditionFee(int grade, string vehicle)
    {
        if (vehicle is not (Vehicles.Bike or Vehicles.Scooter)) return 0;
        switch (grade)
        {
            case 3: 
                throw new ForbiddenVehicleTypeException("Usage of selected vehicle type is forbidden.");;
            case 2: 
                return 1.0;
            case 1: 
                return 0.5;
            default: 
                return 0;
        }
    }
    
    private void ValidateDelivery(Delivery delivery)
    {
        if (delivery.City is null or "") 
            throw new BadRequestException("Provide a valid location."); 
        
        if (delivery.VehicleType is null or "") 
            throw new BadRequestException("Provide a valid vehicle type."); 
        
        if (delivery.DateTime.HasValue)
        {
            if (delivery.DateTime.Value > DateTime.Now)
                throw new BadRequestException("Delivery date cannot be in the future.");
                
            if (delivery.DateTime.Value == DateTime.MinValue)
                throw new BadRequestException($"Invalid date: {delivery.DateTime.Value}");
        }
    }
    
    private void ValidateDeliveryFeeContext(DeliveryFeeContext data, Delivery delivery)
    {
        if (!data.StationId.HasValue)
            throw new NotFoundException($"Weather station for location '{delivery.City}' was not found.");
                
        if (!data.VehicleId.HasValue)
            throw new NotFoundException($"Vehicle type '{delivery.VehicleType}' was not found.");
                
        if (!data.FeeTypeId.HasValue)
            throw new NotFoundException("Regional base fee type was not found.");
                
        if (data.WeatherForecast is null)
        {
            var timestamp = delivery.DateTime.HasValue 
                ? $" at {new DateTimeOffset(delivery.DateTime.Value).ToUnixTimeSeconds()}" 
                : "";
            throw new NotFoundException($"No weather forecast for station '{data.StationId}'{timestamp}.");
        }
    }
}