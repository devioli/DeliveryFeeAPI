using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models.Fee;
using Infrastructure.Persistence.Models.Weather.Station;
using Infrastructure.Persistence.Models.Vehicle;
using Infrastructure.Persistence.Models.Weather.Condition;
using Infrastructure.Persistence.Models.Weather.Forecast;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.UnitTests;

public class RepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IRepository _repository;
    private readonly SqliteConnection _connection;

    public RepositoryTests()
    {
        // Setup in-memory SQLite database
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        // Seed the database
        SeedDatabase(_dbContext);

        // Create repository
        _repository = new Repository(_dbContext);
    }

    [Fact]
    public async Task GetAllWeatherStationsAsync_ReturnsAllStations()
    {
        // Act
        var stations = await _repository.GetAllWeatherStationsAsync();

        // Assert
        Assert.NotNull(stations);
        Assert.NotEmpty(stations);
        Assert.Equal(3, stations.Count());
    }

    [Fact]
    public async Task GetVehicleIdByNameAsync_ReturnsCorrectId()
    {
        // Arrange
        var vehicleName = "car";

        // Act
        var id = await _repository.GetVehicleIdByNameAsync(vehicleName);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        
        // Verify it's the correct ID
        var vehicle = await _dbContext.VehicleTypes.FindAsync(id);
        Assert.NotNull(vehicle);
        Assert.Equal(vehicleName, vehicle!.Name);
    }

    [Fact]
    public async Task GetVehicleIdByNameAsync_WithInvalidName_ReturnsNull()
    {
        // Arrange
        var vehicleName = "nonexistent";

        // Act
        var id = await _repository.GetVehicleIdByNameAsync(vehicleName);

        // Assert
        Assert.Null(id);
    }

    [Fact]
    public async Task GetFeeTypeIdByCodeAsync_ReturnsCorrectId()
    {
        // Arrange
        var feeTypeCode = "rbf";

        // Act
        var id = await _repository.GetFeeTypeIdByCodeAsync(feeTypeCode);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        
        // Verify it's the correct ID
        var feeType = await _dbContext.FeeTypes.FindAsync(id);
        Assert.NotNull(feeType);
        Assert.Equal(feeTypeCode, feeType!.Code);
    }

    [Fact]
    public async Task GetFeeTypeIdByCodeAsync_WithInvalidCode_ReturnsNull()
    {
        // Arrange
        var feeTypeCode = "nonexistent";

        // Act
        var id = await _repository.GetFeeTypeIdByCodeAsync(feeTypeCode);

        // Assert
        Assert.Null(id);
    }

    [Fact]
    public async Task GetWeatherStationIdByCityAsync_ReturnsCorrectId()
    {
        // Arrange
        var city = "tallinn";

        // Act
        var id = await _repository.GetWeatherStationIdByCityAsync(city);

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        
        // Verify it's the correct station
        var location = await _dbContext.Locations
            .FirstOrDefaultAsync(l => l.Name.ToLower() == city.ToLower());
        Assert.NotNull(location);
        Assert.Equal(id, location!.WeatherStationId);
    }

    [Fact]
    public async Task GetWeatherStationIdByCityAsync_WithInvalidCity_ReturnsNull()
    {
        // Arrange
        var city = "nonexistent";

        // Act
        var id = await _repository.GetWeatherStationIdByCityAsync(city);

        // Assert
        Assert.Null(id);
    }

    [Fact]
    public async Task GetRegionalBaseFeeAsync_ReturnsCorrectAmount()
    {
        // Arrange
        var station = await _dbContext.WeatherStations.FirstAsync();
        var vehicleType = await _dbContext.VehicleTypes.FirstAsync();
        var feeType = await _dbContext.FeeTypes.FirstAsync(f => f.Code == "rbf");

        // Act
        var fee = await _repository.GetRegionalBaseFeeAsync(
            station.Id, vehicleType.Id, feeType.Id);

        // Assert
        Assert.True(fee > 0);
        
        // Verify it's the correct fee
        var dbFee = await _dbContext.Fees.FirstAsync(f =>
            f.WeatherStationId == station.Id &&
            f.VehicleTypeId == vehicleType.Id &&
            f.FeeTypeId == feeType.Id);
        Assert.Equal(dbFee.Amount, fee);
    }

    [Fact]
    public async Task GetAllWeatherConditionsAsync_ReturnsAllConditions()
    {
        // Act
        var conditions = await _repository.GetAllWeatherConditionsAsync();

        // Assert
        Assert.NotNull(conditions);
        Assert.NotEmpty(conditions);
    }

    [Fact]
    public async Task GetLatestForecastByStationAsync_ReturnsValidForecast()
    {
        // Arrange
        var station = await _dbContext.WeatherStations.FirstAsync();

        // Act
        var forecast = await _repository.GetLatestForecastByStationAsync(station.Id);

        // Assert
        Assert.NotNull(forecast);
        Assert.Equal(station.Id, forecast.WeatherStationId);
    }

    [Fact]
    public async Task GetForecastByTimeAndStationAsync_ShouldReturnClosestForecast()
    {
        // Arrange
        var station = await _dbContext.WeatherStations.FirstAsync();
        var testDate = DateTime.Today;
        
        // Add multiple forecasts for testing
        var forecasts = new List<WeatherForecast>
        {
            new()
            {
                WeatherStationId = station.Id,
                DateTime = testDate.AddHours(10), // 10:00
                AirTemperature = 10,
                WindSpeed = 10,
                Phenomenon = "clear"
            },
            new()
            {
                WeatherStationId = station.Id,
                DateTime = testDate.AddHours(12), // 12:00
                AirTemperature = 12,
                WindSpeed = 12,
                Phenomenon = "cloudy"
            },
            new()
            {
                WeatherStationId = station.Id,
                DateTime = testDate.AddHours(13), // 13:00
                AirTemperature = 13,
                WindSpeed = 13,
                Phenomenon = "rainy"
            }
        };
        
        await _dbContext.WeatherForecasts.AddRangeAsync(forecasts);
        await _dbContext.SaveChangesAsync();
        
        // Target time is 11:30, so closest forecast should be 12:00
        var targetTime = testDate.AddHours(11).AddMinutes(30);
        
        // Act
        var result = await _repository.GetForecastByTimeAndStationAsync(station.Id, targetTime);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(forecasts[1].DateTime, result.DateTime); // Should be the 12:00 forecast
        Assert.Equal(12, result.AirTemperature);
        Assert.Equal(12, result.WindSpeed);
        Assert.Equal("cloudy", result.Phenomenon);
    }

    private void SeedDatabase(AppDbContext db)
    {
        // Seed stations
        var stations = new List<WeatherStation>
        {
            new WeatherStation { Name = "Tallinn-Harku", WmoCode = 26038 },
            new WeatherStation { Name = "Tartu-Tõravere", WmoCode = 26242 },
            new WeatherStation { Name = "Pärnu", WmoCode = 41803 }
        };
        db.WeatherStations.AddRange(stations);
        db.SaveChanges();

        // Seed locations
        var locations = new List<Location>
        {
            new Location { Name = "tallinn", WeatherStationId = stations[0].Id },
            new Location { Name = "tartu", WeatherStationId = stations[1].Id },
            new Location { Name = "pärnu", WeatherStationId = stations[2].Id }
        };
        db.Locations.AddRange(locations);
        db.SaveChanges();

        // Seed vehicle types
        var vehicleTypes = new List<VehicleType>
        {
            new VehicleType { Name = "car" },
            new VehicleType { Name = "scooter" },
            new VehicleType { Name = "bike" }
        };
        db.VehicleTypes.AddRange(vehicleTypes);
        db.SaveChanges();

        // Seed fee types
        var feeTypes = new List<FeeType>
        {
            new FeeType { Code = "rbf", Name = "Regional Base Fee" },
            new FeeType { Code = "wief", Name = "Weather-dependent Extra Fee" },
            new FeeType { Code = "atef", Name = "Air Temperature Extra Fee" }
        };
        db.FeeTypes.AddRange(feeTypes);
        db.SaveChanges();

        // Seed condition types (severity grades)
        var conditionTypes = new List<ConditionType>
        {
            new ConditionType { Grade = 0 },
            new ConditionType { Grade = 1 },
            new ConditionType { Grade = 2 },
            new ConditionType { Grade = 3 }
        };
        db.ConditionTypes.AddRange(conditionTypes);
        db.SaveChanges();
        
        // Seed conditions
        var conditions = new List<Condition>
        {
            new Condition { Name = "clear", ConditionTypeId = conditionTypes[0].Id },
            new Condition { Name = "few clouds", ConditionTypeId = conditionTypes[0].Id },
            new Condition { Name = "variable clouds", ConditionTypeId = conditionTypes[0].Id },
            new Condition { Name = "light rain", ConditionTypeId = conditionTypes[1].Id },
            new Condition { Name = "moderate rain", ConditionTypeId = conditionTypes[1].Id },
            new Condition { Name = "heavy rain", ConditionTypeId = conditionTypes[2].Id },
            new Condition { Name = "glaze", ConditionTypeId = conditionTypes[3].Id }
        };
        db.Conditions.AddRange(conditions);
        db.SaveChanges();
        
        // Seed regional base fees
        var fees = new List<Fee>
        {
            // Tallinn fees
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[0].Id,
                VehicleTypeId = vehicleTypes[0].Id, Amount = 4.0
            },
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[0].Id,
                VehicleTypeId = vehicleTypes[1].Id, Amount = 3.5
            },
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[0].Id,
                VehicleTypeId = vehicleTypes[2].Id, Amount = 3.0
            },
            
            // Tartu fees
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[1].Id,
                VehicleTypeId = vehicleTypes[0].Id, Amount = 3.5
            },
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[1].Id,
                VehicleTypeId = vehicleTypes[1].Id, Amount = 3.0
            },
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[1].Id,
                VehicleTypeId = vehicleTypes[2].Id, Amount = 2.5
            },
            
            // Pärnu fees
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[2].Id,
                VehicleTypeId = vehicleTypes[0].Id, Amount = 3.0
            },
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[2].Id,
                VehicleTypeId = vehicleTypes[1].Id, Amount = 2.5
            },
            new Fee
            {
                FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[2].Id,
                VehicleTypeId = vehicleTypes[2].Id, Amount = 2.0
            }
        };
        db.Fees.AddRange(fees);
        db.SaveChanges();
        
        // Seed weather forecasts
        var forecasts = new List<WeatherForecast>
        {
            new WeatherForecast
            {
                WeatherStationId = stations[0].Id,
                DateTime = DateTime.UtcNow,
                AirTemperature = 5,
                WindSpeed = 5,
                Phenomenon = "clear"
            },
            new WeatherForecast
            {
                WeatherStationId = stations[1].Id,
                DateTime = DateTime.UtcNow,
                AirTemperature = 5,
                WindSpeed = 4,
                Phenomenon = "few clouds"
            },
            new WeatherForecast
            {
                WeatherStationId = stations[2].Id,
                DateTime = DateTime.UtcNow,
                AirTemperature = 6,
                WindSpeed = 3,
                Phenomenon = "variable clouds"
            }
        };
        db.WeatherForecasts.AddRange(forecasts);
        db.SaveChanges();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
} 