using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Services;
using Moq;
using Xunit;

namespace Tests.UnitTests;

public class WeatherConditionTests
{
    private readonly Mock<IRepository> _mockRepository;
    private readonly Service _service;

    public WeatherConditionTests()
    {
        _mockRepository = new Mock<IRepository>();
        _service = new Service(_mockRepository.Object);
    }
    
    [Theory]
    [InlineData(-11.0, "car", 0.0)] // No extra fee for cars
    [InlineData(-11.0, "scooter", 1.0)] // Below -10C adds 1.0 fee for scooter
    [InlineData(-11.0, "bike", 1.0)] // Below -10C adds 1.0 fee for bike
    [InlineData(-5.0, "scooter", 0.5)] // Between -10C and 0C adds 0.5 fee for scooter
    [InlineData(-5.0, "bike", 0.5)] // Between -10C and 0C adds 0.5 fee for bike
    [InlineData(5.0, "scooter", 0.0)] // Above 0C has no extra fee
    [InlineData(5.0, "bike", 0.0)] // Above 0C has no extra fee
    public void GetAirTemperatureFee_WithDifferentValues_ShouldReturnCorrectFee(
        double temperature, string vehicleType, double expectedFee)
    {
        // Act
        var result = _service.GetAirTemperatureFee(temperature, vehicleType);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Theory]
    [InlineData("bike", 5, 0.0)] // Below 10 m/s, no extra fee
    [InlineData("bike", 15, 0.5)] // Between 10 and 20 m/s, adds 0.5 fee
    [InlineData("scooter", 15, 0.0)] // Scooters not affected by wind
    [InlineData("car", 15, 0.0)] // Cars not affected by wind
    public void GetWindSpeedFee_WithDifferentValues_ShouldReturnCorrectFee(
        string vehicleType, double windSpeed, double expectedFee)
    {
        // Act
        var result = _service.GetWindSpeedFee(windSpeed, vehicleType);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Theory]
    [InlineData("bike", 21)]
    public void GetWindSpeedFee_WithDangerousWindSpeed_ShouldThrowForbiddenVehicleTypeException(
        string vehicleType, double windSpeed)
    {
        // Act & Assert
        Assert.Throws<ForbiddenVehicleTypeException>(() => 
            _service.GetWindSpeedFee(windSpeed, vehicleType));
    }
    
    // [Theory]
    // [InlineData("clear", "bike", 0.0)] // Clear condition, no extra fee
    // [InlineData("rain", "bike", 0.5)] // Grade 1 condition, adds 0.5 fee
    // [InlineData("snow", "bike", 1.0)] // Grade 2 condition, adds 1.0 fee
    // [InlineData("rain", "car", 0.0)] // Cars not affected by weather conditions
    // public async Task GetConditionFee_WithDifferentValues_ShouldReturnCorrectFee(
    //     string phenomenon, string vehicleType, double expectedFee)
    // {
    //     // Arrange
    //     SetupCommonMockRepository(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3.0);
    //     
    //     // Act
    //     var result = await _service.GetConditionFee(phenomenon, vehicleType);
    //     
    //     // Assert
    //     Assert.Equal(expectedFee, result);
    // }
    //
    // [Theory]
    // [InlineData("glaze", "bike")]
    // [InlineData("glaze", "scooter")]
    // public async Task GetConditionFee_WithDangerousCondition_ShouldThrowForbiddenVehicleTypeException(
    //     string phenomenon, string vehicleType)
    // {
    //     // Arrange
    //     SetupCommonMockRepository(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3.0);
    //     
    //     // Act & Assert
    //     await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => 
    //         _service.GetConditionFee(phenomenon, vehicleType));
    // }
} 