using Domain.Constants;
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
            new FeeType { Code = Constants.Fees.Rbf, Name = "Regional Base Fee" },
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
        var conditions = new List<WeatherCondition>
        {
            new WeatherCondition { Name = "clear", ConditionTypeId = conditionTypes[0].Id },
            new WeatherCondition { Name = "few clouds", ConditionTypeId = conditionTypes[0].Id },
            new WeatherCondition { Name = "variable clouds", ConditionTypeId = conditionTypes[0].Id },
            new WeatherCondition { Name = "light rain", ConditionTypeId = conditionTypes[1].Id },
            new WeatherCondition { Name = "moderate rain", ConditionTypeId = conditionTypes[1].Id },
            new WeatherCondition { Name = "heavy rain", ConditionTypeId = conditionTypes[2].Id },
            new WeatherCondition { Name = "glaze", ConditionTypeId = conditionTypes[3].Id }
        };
        db.WeatherConditions.AddRange(conditions);
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