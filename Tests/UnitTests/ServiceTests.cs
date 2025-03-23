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
    public async Task GetDeliveryFeeAsync_ShouldReturnCorrectFeeForCar()
    {
        // Arrange
        const double expectedFee = 4.0;
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "Car",
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync("Car"))
            .ReturnsAsync(vehicleId);
        
        _mockRepository.Setup(repo => repo.GetFeeTypeIdByCodeAsync("rbf"))
            .ReturnsAsync(feeTypeId);
        
        _mockRepository.Setup(repo => repo.GetRegionalBaseFeeAsync(stationId, vehicleId, feeTypeId))
            .ReturnsAsync(expectedFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 20,
                WindSpeed = 5,
                Phenomenon = "Clear"
            });

        // Setup weather conditions for the GetConditionFee method
        _mockRepository.Setup(repo => repo.GetAllWeatherConditionsAsync())
            .ReturnsAsync(new List<WeatherConditionDto>());
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_ShouldReturnCorrectFeeForScooterWithExtraFees()
    {
        // Arrange
        const double baseFee = 3.5;
        const double tempFee = 0.5; // for temperature between -10 and 0
        const double conditionFee = 0.5; // for rainy condition
        const double expectedFee = baseFee + tempFee + conditionFee;
        
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "scooter",
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync("scooter"))
            .ReturnsAsync(vehicleId);
        
        _mockRepository.Setup(repo => repo.GetFeeTypeIdByCodeAsync("rbf"))
            .ReturnsAsync(feeTypeId);
        
        _mockRepository.Setup(repo => repo.GetRegionalBaseFeeAsync(stationId, vehicleId, feeTypeId))
            .ReturnsAsync(baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = -5,
                WindSpeed = 5,
                Phenomenon = "Light rain"
            });
        
        _mockRepository.Setup(repo => repo.GetAllWeatherConditionsAsync())
            .ReturnsAsync(new List<WeatherConditionDto>
            {
                new() {
                    Grade = 1,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "rain" }
                    }
                },
                new() {
                    Grade = 2,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "snow" }
                    }
                },
                new() {
                    Grade = 3,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "glaze" }
                    }
                }
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_ShouldReturnCorrectFeeForBikeWithWindFee()
    {
        // Arrange
        const double baseFee = 3.0;
        const double windFee = 0.5; // for wind between 10 and 20
        const double expectedFee = baseFee + windFee;
        
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "bike",
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync("bike"))
            .ReturnsAsync(vehicleId);
        
        _mockRepository.Setup(repo => repo.GetFeeTypeIdByCodeAsync("rbf"))
            .ReturnsAsync(feeTypeId);
        
        _mockRepository.Setup(repo => repo.GetRegionalBaseFeeAsync(stationId, vehicleId, feeTypeId))
            .ReturnsAsync(baseFee);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15,
                WindSpeed = 15,
                Phenomenon = "Clear"
            });
        
        _mockRepository.Setup(repo => repo.GetAllWeatherConditionsAsync())
            .ReturnsAsync(new List<WeatherConditionDto>
            {
                new() {
                    Grade = 1,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "rain" }
                    }
                },
                new() {
                    Grade = 2,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "snow" }
                    }
                },
                new() {
                    Grade = 3,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "glaze" }
                    }
                }
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(expectedFee, result);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithSpecificDateTime_ShouldUseCorrectForecast()
    {
        // Arrange
        const double baseFee = 3.5;
        var specificDate = new DateTime(2023, 5, 15, 12, 0, 0);
        
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "Car",
            City = "Tallinn",
            DateTime = specificDate
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync("Car"))
            .ReturnsAsync(vehicleId);
        
        _mockRepository.Setup(repo => repo.GetFeeTypeIdByCodeAsync("rbf"))
            .ReturnsAsync(feeTypeId);
        
        _mockRepository.Setup(repo => repo.GetRegionalBaseFeeAsync(stationId, vehicleId, feeTypeId))
            .ReturnsAsync(baseFee);
        
        _mockRepository.Setup(repo => repo.GetForecastByTimeAndStationAsync(stationId, specificDate))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 20,
                WindSpeed = 5,
                Phenomenon = "Clear"
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(baseFee, result);
        _mockRepository.Verify(repo => repo.GetForecastByTimeAndStationAsync(stationId, specificDate), Times.Once);
        _mockRepository.Verify(repo => repo.GetLatestForecastByStationAsync(stationId), Times.Never);
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithForbiddenWeatherCondition_ShouldThrowException()
    {
        // Arrange
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "bike",
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync("bike"))
            .ReturnsAsync(vehicleId);
        
        _mockRepository.Setup(repo => repo.GetFeeTypeIdByCodeAsync("rbf"))
            .ReturnsAsync(feeTypeId);
        
        _mockRepository.Setup(repo => repo.GetRegionalBaseFeeAsync(stationId, vehicleId, feeTypeId))
            .ReturnsAsync(3.0);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15,
                WindSpeed = 5,
                Phenomenon = "Glaze"
            });
        
        _mockRepository.Setup(repo => repo.GetAllWeatherConditionsAsync())
            .ReturnsAsync(new List<WeatherConditionDto>
            {
                new() {
                    Grade = 1,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "rain" }
                    }
                },
                new() {
                    Grade = 2,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "snow" }
                    }
                },
                new() {
                    Grade = 3,
                    Conditions = new List<ConditionDto>
                    {
                        new() { Name = "glaze" }
                    }
                }
            });
        
        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => _service.GetDeliveryFeeAsync(deliveryFee));
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_WithHighWindSpeed_ShouldThrowExceptionForBike()
    {
        // Arrange
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "bike",
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync("bike"))
            .ReturnsAsync(vehicleId);
        
        _mockRepository.Setup(repo => repo.GetFeeTypeIdByCodeAsync("rbf"))
            .ReturnsAsync(feeTypeId);
        
        _mockRepository.Setup(repo => repo.GetRegionalBaseFeeAsync(stationId, vehicleId, feeTypeId))
            .ReturnsAsync(3.0);
        
        _mockRepository.Setup(repo => repo.GetLatestForecastByStationAsync(stationId))
            .ReturnsAsync(new WeatherForecastDto
            {
                AirTemperature = 15,
                WindSpeed = 21, // Higher than allowed
                Phenomenon = "Clear"
            });
        
        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenVehicleTypeException>(() => _service.GetDeliveryFeeAsync(deliveryFee));
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_ShouldThrowNotFoundForInvalidCity()
    {
        // Arrange
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "Car",
            City = "NonExistentCity"
        };
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("NonExistentCity"))
            .ReturnsAsync((Guid?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetDeliveryFeeAsync(deliveryFee));
    }
    
    [Fact]
    public async Task GetDeliveryFeeAsync_ShouldThrowNotFoundForInvalidVehicle()
    {
        // Arrange
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "NonExistentVehicle",
            City = "Tallinn"
        };
        
        var stationId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetWeatherStationIdByCityAsync("Tallinn"))
            .ReturnsAsync(stationId);
        
        _mockRepository.Setup(repo => repo.GetVehicleIdByNameAsync("NonExistentVehicle"))
            .ReturnsAsync((Guid?)null);
        
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetDeliveryFeeAsync(deliveryFee));
    }

    [Fact]
    public async Task GetDeliveryFeeAsync_ShouldSelectClosestForecastWhenTimestampProvided()
    {
        // Arrange
        var baseDate = new DateTime(2023, 5, 15);
        var forecasts = new List<WeatherForecastDto>
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
        
        var deliveryFee = new DeliveryFee
        {
            VehicleType = "car",
            City = "tallinn",
            DateTime = targetTime
        };
        
        var stationId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var feeTypeId = Guid.NewGuid();
        
        _mockRepository.Setup(repo => repo.GetDeliveryFeeDataAsync("tallinn", "car", targetTime))
            .ReturnsAsync(new DeliveryFeeDataDto
            {
                StationId = stationId,
                VehicleId = vehicleId,
                FeeTypeId = feeTypeId,
                RegionalBaseFee = 4.0,
                WeatherForecast = expectedForecast,
                WeatherConditions = new List<WeatherConditionDto>()
            });
        
        // Act
        var result = await _service.GetDeliveryFeeAsync(deliveryFee);
        
        // Assert
        Assert.Equal(4.0, result); // Base fee for car with no extra fees
        _mockRepository.Verify(repo => repo.GetDeliveryFeeDataAsync("tallinn", "car", targetTime), Times.Once);
    }
} 