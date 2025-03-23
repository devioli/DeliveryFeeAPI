using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
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
    [InlineData(-11.0, "car", 0.0)] // Temperature below -10C has no effect on car fee
    [InlineData(-11.0, "scooter", 1.0)] // Temperature below -10C adds 1.0 fee for scooter
    [InlineData(-11.0, "bike", 1.0)] // Temperature below -10C adds 1.0 fee for bike
    [InlineData(-5.0, "scooter", 0.5)] // Temperature between -10C and 0C adds 0.5 fee for scooter
    [InlineData(-5.0, "bike", 0.5)] // Temperature between -10C and 0C adds 0.5 fee for bike
    [InlineData(5.0, "scooter", 0.0)] // Temperature above 0C has no extra fee for scooter
    [InlineData(5.0, "bike", 0.0)] // Temperature above 0C has no extra fee for bike
    public async Task GetDeliveryFeeAsync_WithDifferentTemperatures_ShouldApplyCorrectTemperatureFee(
        double temperature, string vehicleType, double expectedFee)
    {
        // Arrange
        var deliveryFee = new Domain.Models.DeliveryFee
        {
            VehicleType = vehicleType,
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        const double baseFee = 3.0;
        
        SetupCommonMockRepository(stationId, vehicleId, feeTypeId, baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = temperature,
                WindSpeed = 5.0,
                Phenomenon = "Clear"
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(baseFee + expectedFee, result);
    }
    
    [Theory]
    [InlineData("bike", 15, 0.5)] // Wind speed between 10 and 20, adds 0.5 fee for bike
    [InlineData("bike", 5, 0.0)] // Wind speed below 10, no extra fee for bike
    // Bike with wind speed > 20 is now tested in the dangerous conditions test
    [InlineData("scooter", 15, 0.0)] // Scooters are not affected by wind speed
    [InlineData("scooter", 5, 0.0)] // Scooters are not affected by wind speed
    [InlineData("scooter", 21, 0.0)] // Scooters are not affected by wind speed
    [InlineData("car", 15, 0.0)] // Cars are not affected by wind speed
    [InlineData("car", 21, 0.0)] // Cars are not affected by wind speed
    public async Task GetDeliveryFeeAsync_WithDifferentWindSpeeds_ShouldApplyCorrectWindSpeedFee(
        string vehicleType, double windSpeed, double expectedFee)
    {
        // Arrange
        var deliveryFee = new Domain.Models.DeliveryFee
        {
            VehicleType = vehicleType,
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        const double baseFee = 3.0;
        
        SetupCommonMockRepository(stationId, vehicleId, feeTypeId, baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15.0,
                WindSpeed = windSpeed,
                Phenomenon = "Clear"
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(baseFee + expectedFee, result);
    }
    
    [Theory]
    [InlineData("bike", 21)] // Wind speed above 20 is forbidden for bikes
    public async Task GetDeliveryFeeAsync_WithDangerousWindSpeed_ShouldThrowForbiddenVehicleTypeException(
        string vehicleType, double windSpeed)
    {
        // Arrange
        var deliveryFee = new Domain.Models.DeliveryFee
        {
            VehicleType = vehicleType,
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        const double baseFee = 3.0;
        
        SetupCommonMockRepository(stationId, vehicleId, feeTypeId, baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15.0,
                WindSpeed = windSpeed,
                Phenomenon = "Clear"
            });
        
        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => 
            _service.GetDeliveryFeeAsync(deliveryFee));
    }
    
    [Theory]
    [InlineData("Clear", 0.0)] // Clear phenomenon, no extra fee
    [InlineData("RAIN", 0.0)] // Car is not affected by rain
    [InlineData("snow", 0.0)] // Car is not affected by snow
    // Glaze test is now in the dangerous conditions test
    public async Task GetDeliveryFeeAsync_WithDifferentWeatherConditions_ForCar_ShouldNotApplyWeatherFee(
        string phenomenon, double expectedFee)
    {
        // Arrange
        var deliveryFee = new Domain.Models.DeliveryFee
        {
            VehicleType = "car", // Cars are not affected by weather conditions
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        const double baseFee = 3.0;
        
        SetupCommonMockRepository(stationId, vehicleId, feeTypeId, baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15.0,
                WindSpeed = 5.0,
                Phenomenon = phenomenon
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(baseFee + expectedFee, result);
    }
    
    [Theory]
    [InlineData("Clear", 0.0)] // Clear phenomenon, no extra fee
    [InlineData("RAIN", 0.5)] // Rain phenomenon (case insensitive), grade 1, adds 0.5 fee
    [InlineData("snow", 1.0)] // Snow phenomenon, grade 2, adds 1.0 fee
    public async Task GetDeliveryFeeAsync_WithDifferentWeatherConditions_ForScooter_ShouldApplyWeatherFee(
        string phenomenon, double expectedFee)
    {
        // Arrange
        var deliveryFee = new Domain.Models.DeliveryFee
        {
            VehicleType = "scooter", // Scooters are affected by weather conditions
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        const double baseFee = 3.0;
        
        SetupCommonMockRepository(stationId, vehicleId, feeTypeId, baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15.0,
                WindSpeed = 5.0,
                Phenomenon = phenomenon
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(baseFee + expectedFee, result);
    }
    
    [Theory]
    [InlineData("scooter", "Glaze")] // Dangerous condition for scooters
    [InlineData("bike", "Glaze")] // Dangerous condition for bikes
    public async Task GetDeliveryFeeAsync_WithDangerousWeatherCondition_ShouldThrowForbiddenVehicleTypeException(
        string vehicleType, string phenomenon)
    {
        // Arrange
        var deliveryFee = new Domain.Models.DeliveryFee
        {
            VehicleType = vehicleType,
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        const double baseFee = 3.0;
        
        SetupCommonMockRepository(stationId, vehicleId, feeTypeId, baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15.0,
                WindSpeed = 5.0,
                Phenomenon = phenomenon
            });
        
        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => 
            _service.GetDeliveryFeeAsync(deliveryFee));
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
    
    [Theory]
    [InlineData("clear", "bike", 0.0)] // Clear condition, no extra fee
    [InlineData("rain", "bike", 0.5)] // Grade 1 condition, adds 0.5 fee
    [InlineData("snow", "bike", 1.0)] // Grade 2 condition, adds 1.0 fee
    [InlineData("rain", "car", 0.0)] // Cars not affected by weather conditions
    public async Task GetConditionFee_WithDifferentValues_ShouldReturnCorrectFee(
        string phenomenon, string vehicleType, double expectedFee)
    {
        // Arrange
        SetupCommonMockRepository(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3.0);
        
        // Act
        var result = await _service.GetConditionFee(phenomenon, vehicleType);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Theory]
    [InlineData("glaze", "bike")]
    [InlineData("glaze", "scooter")]
    public async Task GetConditionFee_WithDangerousCondition_ShouldThrowForbiddenVehicleTypeException(
        string phenomenon, string vehicleType)
    {
        // Arrange
        SetupCommonMockRepository(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3.0);
        
        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => 
            _service.GetConditionFee(phenomenon, vehicleType));
    }
    
    /// <summary>
    /// Helper method to set up common mock repository responses
    /// </summary>
    private void SetupCommonMockRepository(Guid stationId, Guid vehicleId, Guid feeTypeId, double baseFee)
    {
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(vehicleId);
        
        _mockRepository.Setup(repo => repo.GetFeeTypeIdByCodeAsync("rbf"))
            .ReturnsAsync(feeTypeId);
        
        _mockRepository.Setup(repo => repo.GetRegionalBaseFeeAsync(stationId, vehicleId, feeTypeId))
            .ReturnsAsync(baseFee);
        
        _mockRepository.Setup(repo => repo.GetAllWeatherConditionsAsync())
            .ReturnsAsync(new List<WeatherConditionDto>
            {
                new()
                {
                    Grade = 1,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "rain" }
                    }
                },
                new()
                {
                    Grade = 2,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "snow" }
                    }
                },
                new()
                {
                    Grade = 3,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "glaze" }
                    }
                }
            });
    }
} 