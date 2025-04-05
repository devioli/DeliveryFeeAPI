using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Services;
using Moq;
using Xunit;
using static Domain.Constants.Constants;

namespace Tests;

public class ServiceTests
{
    private readonly Mock<IRepository> _mockRepository;
    private readonly Service _service;

    public ServiceTests()
    {
        _mockRepository = new Mock<IRepository>();
        _service = new Service(_mockRepository.Object);
    }
    
    #region Regional Base Fee Tests

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Car, 4.0)]
    [InlineData(Locations.Tallinn, Vehicles.Scooter, 3.5)]
    [InlineData(Locations.Tallinn, Vehicles.Bike, 3.0)]
    [InlineData(Locations.Tartu, Vehicles.Car, 3.5)]
    [InlineData(Locations.Tartu, Vehicles.Scooter, 3.0)]
    [InlineData(Locations.Tartu, Vehicles.Bike, 2.5)]
    [InlineData(Locations.Pärnu, Vehicles.Car, 3.0)]
    [InlineData(Locations.Pärnu, Vehicles.Scooter, 2.5)]
    [InlineData(Locations.Pärnu, Vehicles.Bike, 2.0)]
    public async Task GetDeliveryFeeAsync_RegionalBaseFeeOnly_ReturnsCorrectBaseFee(string city, string vehicleType, double expectedFee)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var context = CreateDeliveryFeeContext(regionalBaseFee: expectedFee);
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }

    #endregion

    #region Air Temperature Extra Fee Tests

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Bike, -15, 1.0, 4.0)]
    [InlineData(Locations.Tallinn, Vehicles.Scooter, -12, 1.0, 4.5)]
    [InlineData(Locations.Tallinn, Vehicles.Car, -20, 0.0, 4.0)]
    [InlineData(Locations.Tartu, Vehicles.Bike, -5, 0.5, 3.0)]
    [InlineData(Locations.Tartu, Vehicles.Scooter, -8, 0.5, 3.5)]
    [InlineData(Locations.Pärnu, Vehicles.Bike, 0, 0.0, 2.0)]
    [InlineData(Locations.Pärnu, Vehicles.Scooter, 5, 0.0, 2.5)]
    public async Task GetDeliveryFeeAsync_WithAirTemperatureExtraFee_ReturnsCorrectTotalFee(
        string city, string vehicleType, double temperature, double expectedExtraFee, double expectedTotalFee)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var baseRegionalFee = expectedTotalFee - expectedExtraFee;
        var context = CreateDeliveryFeeContext(regionalBaseFee: baseRegionalFee, airTemperature: temperature);
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedTotalFee, result);
    }

    #endregion

    #region Wind Speed Extra Fee Tests

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Bike, 15, 0.5, 3.5)]
    [InlineData(Locations.Tartu, Vehicles.Bike, 12, 0.5, 3.0)]
    [InlineData(Locations.Pärnu, Vehicles.Bike, 18, 0.5, 2.5)]
    [InlineData(Locations.Tallinn, Vehicles.Bike, 5, 0.0, 3.0)]
    [InlineData(Locations.Tallinn, Vehicles.Scooter, 15, 0.0, 3.5)]
    [InlineData(Locations.Tallinn, Vehicles.Car, 15, 0.0, 4.0)]
    public async Task GetDeliveryFeeAsync_WithWindSpeedExtraFee_ReturnsCorrectTotalFee(
        string city, string vehicleType, double windSpeed, double expectedExtraFee, double expectedTotalFee)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var baseRegionalFee = expectedTotalFee - expectedExtraFee;
        var context = CreateDeliveryFeeContext(regionalBaseFee: baseRegionalFee, windSpeed: windSpeed);
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedTotalFee, result);
    }

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Bike, 21)]
    [InlineData(Locations.Tartu, Vehicles.Bike, 25)]
    [InlineData(Locations.Pärnu, Vehicles.Bike, 30)]
    public async Task GetDeliveryFeeAsync_WithHighWindSpeed_ThrowsForbiddenVehicleTypeException(
        string city, string vehicleType, double windSpeed)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var context = CreateDeliveryFeeContext(windSpeed: windSpeed);
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
            
        Assert.Equal("Usage of selected vehicle type is forbidden.", exception.Message);
    }

    #endregion

    #region Weather Phenomenon Extra Fee Tests

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Bike, 1, 0.5, 3.5)]
    [InlineData(Locations.Tallinn, Vehicles.Scooter, 1, 0.5, 4.0)]
    [InlineData(Locations.Tartu, Vehicles.Bike, 2, 1.0, 3.5)]
    [InlineData(Locations.Tartu, Vehicles.Scooter, 2, 1.0, 4.0)]
    [InlineData(Locations.Pärnu, Vehicles.Bike, 0, 0.0, 2.0)]
    [InlineData(Locations.Pärnu, Vehicles.Car, 2, 0.0, 3.0)]
    public async Task GetDeliveryFeeAsync_WithWeatherPhenomenonExtraFee_ReturnsCorrectTotalFee(
        string city, string vehicleType, int weatherGrade, double expectedExtraFee, double expectedTotalFee)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var baseRegionalFee = expectedTotalFee - expectedExtraFee;
        var context = CreateDeliveryFeeContext(regionalBaseFee: baseRegionalFee, weatherGrade: weatherGrade);
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedTotalFee, result);
    }

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Bike, 3)]
    [InlineData(Locations.Tallinn, Vehicles.Scooter, 3)]
    [InlineData(Locations.Tartu, Vehicles.Bike, 3)]
    [InlineData(Locations.Tartu, Vehicles.Scooter, 3)]
    public async Task GetDeliveryFeeAsync_WithSevereWeatherPhenomenon_ThrowsForbiddenVehicleTypeException(
        string city, string vehicleType, int weatherGrade)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var context = CreateDeliveryFeeContext(weatherGrade: weatherGrade);
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
            
        Assert.Equal("Usage of selected vehicle type is forbidden.", exception.Message);
    }

    #endregion

    #region Combined Fee Tests

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Bike, -5, 15, 2, 0.5, 0.5, 1.0, 5.0)]
    [InlineData(Locations.Tallinn, Vehicles.Scooter, -8, 12, 1, 0.5, 0.0, 0.5, 4.5)]
    [InlineData(Locations.Tallinn, Vehicles.Car, -20, 25, 3, 0.0, 0.0, 0.0, 4.0)]
    [InlineData(Locations.Tartu, Vehicles.Bike, -2.1, 4.7, 2, 0.5, 0.0, 1.0, 4.0)]
    [InlineData(Locations.Tartu, Vehicles.Scooter, -6, 7, 2, 0.5, 0.0, 1.0, 4.5)]
    [InlineData(Locations.Tartu, Vehicles.Car, -12, 18, 1, 0.0, 0.0, 0.0, 3.5)]
    [InlineData(Locations.Pärnu, Vehicles.Bike, -11, 14, 1, 1.0, 0.5, 0.5, 4.0)]
    [InlineData(Locations.Pärnu, Vehicles.Scooter, -15, 8, 1, 1.0, 0.0, 0.5, 4.0)]
    [InlineData(Locations.Pärnu, Vehicles.Car, -7, 22, 2, 0.0, 0.0, 0.0, 3.0)]
    public async Task GetDeliveryFeeAsync_WithCombinedFees_ReturnsCorrectTotalFee(
        string city, string vehicleType, double temperature, double windSpeed, int weatherGrade, 
        double expectedAirTempFee, double expectedWindFee, double expectedWeatherFee, double expectedTotalFee)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var baseRegionalFee = expectedTotalFee - (expectedAirTempFee + expectedWindFee + expectedWeatherFee);
        var context = CreateDeliveryFeeContext(
            regionalBaseFee: baseRegionalFee, 
            airTemperature: temperature, 
            windSpeed: windSpeed, 
            weatherGrade: weatherGrade);
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedTotalFee, result);
    }

    #endregion

    #region Individual Fee Calculation Method Tests

    [Theory]
    [InlineData(-11, Vehicles.Bike, 1)]
    [InlineData(-11, Vehicles.Scooter, 1)]
    [InlineData(-5, Vehicles.Bike, 0.5)]
    [InlineData(-5, Vehicles.Scooter, 0.5)]
    [InlineData(0, Vehicles.Bike, 0)]
    [InlineData(0, Vehicles.Scooter, 0)]
    [InlineData(10, Vehicles.Bike, 0)]
    [InlineData(10, Vehicles.Scooter, 0)]
    [InlineData(-20, Vehicles.Car, 0)]
    public void GetAirTemperatureFee_ReturnsCorrectFee(double temperature, string vehicleType, double expectedFee)
    {
        // Act
        var result = _service.GetAirTemperatureFee(temperature, vehicleType);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Theory]
    [InlineData(5, Vehicles.Bike, 0)]
    [InlineData(10, Vehicles.Bike, 0)]
    [InlineData(15, Vehicles.Bike, 0.5)]
    [InlineData(19, Vehicles.Bike, 0.5)]
    [InlineData(5, Vehicles.Car, 0)]
    [InlineData(15, Vehicles.Car, 0)]
    [InlineData(5, Vehicles.Scooter, 0)]
    [InlineData(15, Vehicles.Scooter, 0)]
    public void GetWindSpeedFee_ReturnsCorrectFee(double windSpeed, string vehicleType, double expectedFee)
    {
        // Act
        var result = _service.GetWindSpeedFee(windSpeed, vehicleType);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Fact]
    public void GetWindSpeedFee_ForBikeWithHighWindSpeed_ThrowsForbiddenVehicleTypeException()
    {
        // Act & Assert
        var exception = Assert.Throws<ForbiddenVehicleTypeException>(() => 
            _service.GetWindSpeedFee(21, Vehicles.Bike));
            
        Assert.Equal("Usage of selected vehicle type is forbidden.", exception.Message);
    }
    
    [Theory]
    [InlineData(0, Vehicles.Bike, 0)]
    [InlineData(0, Vehicles.Scooter, 0)]
    [InlineData(1, Vehicles.Bike, 0.5)]
    [InlineData(1, Vehicles.Scooter, 0.5)]
    [InlineData(2, Vehicles.Bike, 1.0)]
    [InlineData(2, Vehicles.Scooter, 1.0)]
    [InlineData(0, Vehicles.Car, 0)]
    [InlineData(1, Vehicles.Car, 0)]
    [InlineData(2, Vehicles.Car, 0)]
    public void GetConditionFee_ReturnsCorrectFee(int grade, string vehicleType, double expectedFee)
    {
        // Act
        var result = _service.GetConditionFee(grade, vehicleType);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Theory]
    [InlineData(Vehicles.Bike)]
    [InlineData(Vehicles.Scooter)]
    public void GetConditionFee_ForBikeAndScooterWithSevereConditions_ThrowsForbiddenVehicleTypeException(string vehicleType)
    {
        // Act & Assert
        var exception = Assert.Throws<ForbiddenVehicleTypeException>(() => 
            _service.GetConditionFee(3, vehicleType));
            
        Assert.Equal("Usage of selected vehicle type is forbidden.", exception.Message);
    }
    
    [Fact]
    public void GetConditionFee_ForCarWithSevereConditions_ReturnsZero()
    {
        // Act
        var result = _service.GetConditionFee(3, Vehicles.Car);
        
        // Assert
        Assert.Equal(0, result);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task GetDeliveryFeeAsync_NullCity_ThrowsBadRequestException()
    {
        // Arrange
        var delivery = CreateDelivery(city: null);
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_EmptyCity_ThrowsBadRequestException()
    {
        // Arrange
        var delivery = CreateDelivery(city: "");
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_NullVehicleType_ThrowsBadRequestException()
    {
        // Arrange
        var delivery = CreateDelivery(vehicleType: null);
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_EmptyVehicleType_ThrowsBadRequestException()
    {
        // Arrange
        var delivery = CreateDelivery(vehicleType: "");
        
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
    }

    #endregion

    #region Missing Data Tests

    [Fact]
    public async Task GetDeliveryFeeAsync_NoStationId_ThrowsNotFoundException()
    {
        // Arrange
        var delivery = CreateDelivery();
        var context = CreateDeliveryFeeContext(hasStationId: false);
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
        
        Assert.Equal($"Weather station for location '{delivery.City}' was not found.", exception.Message);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_NoVehicleId_ThrowsNotFoundException()
    {
        // Arrange
        var delivery = CreateDelivery();
        var context = CreateDeliveryFeeContext(hasVehicleId: false);
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
        
        Assert.Equal($"Vehicle type '{delivery.VehicleType}' was not found.", exception.Message);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_NoFeeTypeId_ThrowsNotFoundException()
    {
        // Arrange
        var delivery = CreateDelivery();
        var context = CreateDeliveryFeeContext(hasFeeTypeId: false);
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
        
        Assert.Equal("Regional base fee type was not found.", exception.Message);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_NoWeatherForecast_ThrowsNotFoundException()
    {
        // Arrange
        var delivery = CreateDelivery();
        var context = CreateDeliveryFeeContext(hasWeatherForecast: false);
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
        
        Assert.Equal($"No weather forecast for station '{context.StationId}'.", exception.Message);
    }

    #endregion

    #region Timestamp Tests

    [Fact]
    public async Task GetDeliveryFeeAsync_WithSpecificDateTime_PassesDateTimeToRepository()
    {
        // Arrange
        var timestamp = new DateTime(2025, 3, 15, 10, 30, 0);
        var delivery = CreateDelivery(Locations.Tallinn, Vehicles.Car, timestamp);
        var context = CreateDeliveryFeeContext();
        SetupVerifiableRepositoryMock(delivery, context);
        
        // Act
        await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        _mockRepository.Verify();
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithNullDateTime_PassesNullToRepository()
    {
        // Arrange
        var delivery = CreateDelivery(Locations.Tallinn, Vehicles.Car);
        var context = CreateDeliveryFeeContext();
        SetupVerifiableRepositoryMock(delivery, context);
        
        // Act
        await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        _mockRepository.Verify();
    }

    #endregion

    #region Date Validation Tests

    [Fact]
    public async Task GetDeliveryFeeAsync_WithFutureDate_ThrowsBadRequestException()
    {
        // Arrange
        var futureDate = DateTime.Now.AddDays(1);
        var delivery = CreateDelivery(dateTime: futureDate);
        var context = CreateDeliveryFeeContext();
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
            
        Assert.Equal("Delivery date cannot be in the future.", exception.Message);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithDateTimeMinValue_HandlesGracefully()
    {
        // Arrange
        var delivery = CreateDelivery(dateTime: DateTime.MinValue);
        var context = CreateDeliveryFeeContext();
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        try
        {
            var result = await _service.GetDeliveryFeeAsync(delivery);
            Assert.Equal(3.0, result);
        }
        catch (Exception ex)
        {
            var badRequestException = Assert.IsType<BadRequestException>(ex);
            Assert.Equal($"Invalid date: {delivery.DateTime}", badRequestException.Message);
        }
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithDateTimeMaxValue_HandlesGracefully()
    {
        // Arrange
        var delivery = CreateDelivery(dateTime: DateTime.MaxValue);
        var context = CreateDeliveryFeeContext();
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
            
        Assert.Equal("Delivery date cannot be in the future.", exception.Message);
    }

    [Fact]
    public async Task GetDeliveryFeeAsync_WithReasonablePastDate_SucceedsWithCorrectFee()
    {
        // Arrange
        var pastDate = DateTime.Now.AddDays(-30);
        var delivery = CreateDelivery(dateTime: pastDate);
        var context = CreateDeliveryFeeContext();
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(3.0, result);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithOldDate_SucceedsWithCorrectFee()
    {
        // Arrange
        var oldDate = DateTime.Now.AddYears(-3);
        var delivery = CreateDelivery(dateTime: oldDate);
        var context = CreateDeliveryFeeContext();
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(3.0, result);
    }

    #endregion

    #region Case Sensitivity Tests

    [Theory]
    [InlineData("TALLINN", "BIKE", 3.0)]
    [InlineData("TaLLiNn", "BiKe", 3.0)]
    [InlineData("tallinn", "BIKE", 3.0)]
    [InlineData("TARTU", "SCOOTER", 3.0)]
    [InlineData("PÄRNU", "CAR", 3.0)]
    public async Task GetDeliveryFeeAsync_WithDifferentCasing_HandlesCorrectly(string city, string vehicleType, double expectedFee)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var context = CreateDeliveryFeeContext(regionalBaseFee: expectedFee);
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }

    #endregion

    #region Special Character Tests

    [Theory]
    [InlineData("Pärnu", Vehicles.Bike, 2.0)]
    [InlineData("Võru", Vehicles.Bike, 2.0)]
    public async Task GetDeliveryFeeAsync_WithSpecialCharacters_HandlesCorrectly(string city, string vehicleType, double expectedFee)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var context = CreateDeliveryFeeContext(regionalBaseFee: expectedFee);
        SetupRepositoryMock(delivery, context);
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }

    #endregion

    #region Multiple Forbidden Conditions Tests

    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Bike, 25, 3, "Usage of selected vehicle type is forbidden.")]
    [InlineData(Locations.Tartu, Vehicles.Bike, 22, 3, "Usage of selected vehicle type is forbidden.")]
    [InlineData(Locations.Pärnu, Vehicles.Scooter, 15, 3, "Usage of selected vehicle type is forbidden.")]
    public async Task GetDeliveryFeeAsync_WithMultipleForbiddenConditions_ThrowsForbiddenVehicleTypeException(
        string city, string vehicleType, double windSpeed, int weatherGrade, string expectedMessage)
    {
        // Arrange
        var delivery = CreateDelivery(city, vehicleType);
        var context = CreateDeliveryFeeContext(
            windSpeed: windSpeed,
            weatherGrade: weatherGrade);
        SetupRepositoryMock(delivery, context);
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => 
            _service.GetDeliveryFeeAsync(delivery));
            
        Assert.Equal(expectedMessage, exception.Message);
    }

    #endregion

    #region Floating-Point Precision Tests

    [Theory]
    [InlineData(-4.9999999, 0.5)]
    [InlineData(-5.0000001, 0.5)]
    [InlineData(-0.0000001, 0.5)]
    [InlineData(0.0000001, 0)]
    public void GetAirTemperatureFee_WithFloatingPointEdgeCases_ReturnsCorrectFee(double temperature, double expectedFee)
    {
        // Act
        var result = _service.GetAirTemperatureFee(temperature, Vehicles.Bike);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }

    [Theory]
    [InlineData(9.9999999, 0)]
    [InlineData(10.0000001, 0.5)]
    [InlineData(19.9999999, 0.5)]
    public void GetWindSpeedFee_WithFloatingPointEdgeCases_ReturnsCorrectFee(double windSpeed, double expectedFee)
    {
        // Act
        var result = _service.GetWindSpeedFee(windSpeed, Vehicles.Bike);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }

    [Fact]
    public void GetWindSpeedFee_WithJustAbove20_ThrowsForbiddenVehicleTypeException()
    {
        // Act & Assert
        var exception = Assert.Throws<ForbiddenVehicleTypeException>(() => 
            _service.GetWindSpeedFee(20.0000001, Vehicles.Bike));
            
        Assert.Equal("Usage of selected vehicle type is forbidden.", exception.Message);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithTinyFractionalFees_CalculatesCorrectly()
    {
        // Arrange
        var delivery = CreateDelivery(Locations.Tallinn, Vehicles.Bike);
        
        var preciseBaseFee = 2.95;
        var context = CreateDeliveryFeeContext(
            regionalBaseFee: preciseBaseFee,
            airTemperature: -5.01,
            windSpeed: 10.01,
            weatherGrade: 1);
            
        SetupRepositoryMock(delivery, context);
        
        var expectedTotal = preciseBaseFee + 0.5 + 0.5 + 0.5;
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(delivery);
        
        // Assert
        Assert.Equal(expectedTotal, result);
    }

    #endregion
    
    #region Test Helpers
    
    private Delivery CreateDelivery(string? city = Locations.Tallinn, string? vehicleType = Vehicles.Car, DateTime? dateTime = null)
    {
        return new Delivery
        {
            City = city!,
            VehicleType = vehicleType!,
            DateTime = dateTime
        };
    }
    
    private DeliveryFeeContext CreateDeliveryFeeContext(
        double regionalBaseFee = 3.0,
        double airTemperature = 10,
        double windSpeed = 5,
        int weatherGrade = 0,
        string? phenomenon = null,
        bool hasStationId = true,
        bool hasVehicleId = true,
        bool hasFeeTypeId = true,
        bool hasWeatherForecast = true)
    {
        var context = new DeliveryFeeContext
        {
            StationId = hasStationId ? Guid.NewGuid() : null,
            VehicleId = hasVehicleId ? Guid.NewGuid() : null,
            FeeTypeId = hasFeeTypeId ? Guid.NewGuid() : null,
            RegionalBaseFee = regionalBaseFee,
            WeatherConditionGrade = weatherGrade
        };
        
        if (hasWeatherForecast)
        {
            context.WeatherForecast = new ForecastDto
            {
                AirTemperature = airTemperature,
                WindSpeed = windSpeed,
                Phenomenon = phenomenon
            };
        }
        
        return context;
    }
    
    private void SetupRepositoryMock(Delivery delivery, DeliveryFeeContext context)
    {
        _mockRepository.Setup(repo => 
            repo.GetDeliveryFeeContextAsync(
                delivery.City, 
                delivery.VehicleType, 
                delivery.DateTime))
            .ReturnsAsync(context);
    }
    
    private void SetupVerifiableRepositoryMock(Delivery delivery, DeliveryFeeContext context)
    {
        _mockRepository.Setup(repo => 
            repo.GetDeliveryFeeContextAsync(
                delivery.City, 
                delivery.VehicleType, 
                delivery.DateTime))
            .ReturnsAsync(context)
            .Verifiable();
    }
    
    #endregion
} 