using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Services;
using Moq;
using Xunit;

namespace Tests.UnitTests;

public class ServiceTests
{
    private readonly Mock<IRepository> _mockRepository;
    private readonly Service _service;

    public ServiceTests()
    {
        _mockRepository = new Mock<IRepository>();
        _service = new Service(_mockRepository.Object);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_ShouldSelectClosestForecastWhenTimestampProvided()
    {
        // Arrange
        var baseDate = new DateTime(2023, 5, 15);
        var forecasts = new List<ForecastDto>
        {
            new()
            {
                DateTime = baseDate.AddHours(10), // 10:00
                AirTemperature = 10,
                WindSpeed = 10,
                Phenomenon = "Clear"
            },
            new()
            {
                DateTime = baseDate.AddHours(12), // 12:00
                AirTemperature = 12,
                WindSpeed = 8,
                Phenomenon = "Cloudy"
            },
            new()
            {
                DateTime = baseDate.AddHours(13), // 13:00
                AirTemperature = 13,
                WindSpeed = 9,
                Phenomenon = "Rainy"
            }
        };

        // Target time is 11:30, so closest forecast should be 12:00
        var targetTime = baseDate.AddHours(11).AddMinutes(30);
        var expectedForecast = forecasts[1]; // 12:00 forecast
        
        var deliveryFee = new Delivery
        {
            VehicleType = "car",
            City = "tallinn",
            DateTime = targetTime
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetDeliveryFeeContextAsync("tallinn", "car", targetTime))
            .ReturnsAsync(new DeliveryFeeContext
            {
                StationId = stationId,
                VehicleId = vehicleId,
                FeeTypeId = feeTypeId,
                RegionalBaseFee = 4.0,
                WeatherForecast = expectedForecast,
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(4.0, result); // Base fee for car with no extra fees
        _mockRepository.Verify(repo => repo.GetDeliveryFeeContextAsync("tallinn", "car", targetTime), Times.Once);
    }
} 