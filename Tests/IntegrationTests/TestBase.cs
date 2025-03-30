using Domain.Constants;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models.Fee;
using Infrastructure.Persistence.Models.Weather.Station;
using Infrastructure.Persistence.Models.Vehicle;
using Infrastructure.Persistence.Models.Weather.Condition;
using Infrastructure.Persistence.Models.Weather.Forecast;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.IntegrationTests
{
    /// <summary>
    /// Base class for integration tests with a SQLite in-memory database
    /// </summary>
    public class TestBase : IDisposable
    {
        protected readonly SqliteConnection Connection;
        protected readonly AppDbContext DbContext;

        public TestBase()
        {
            // Create an in-memory SQLite database connection
            Connection = new SqliteConnection("DataSource=:memory:");
            Connection.Open();

            // Set up DbContext with in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(Connection)
                .Options;

            DbContext = new AppDbContext(options);
            DbContext.Database.EnsureCreated();
            
            // Seed test data
            SeedTestData();
        }

        /// <summary>
        /// Creates and returns a new SQLite in-memory connection for use in tests
        /// </summary>
        public static SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            // Set up DbContext with in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var dbContext = new AppDbContext(options))
            {
                dbContext.Database.EnsureCreated();
            }

            return connection;
        }

        /// <summary>
        /// Seeds the database with test data
        /// </summary>
        public void SeedTestData()
        {
            // Seed weather stations
            var stations = new[]
            {
                new WeatherStation { Name = "Tallinn-Harku", WmoCode = 26038 },
                new WeatherStation { Name = "Tartu-Tõravere", WmoCode = 26242 },
                new WeatherStation { Name = "Pärnu", WmoCode = 41803 }
            };
            DbContext.WeatherStations.AddRange(stations);
            DbContext.SaveChanges();

            // Seed locations
            var locations = new[]
            {
                new Location { Name = "tallinn", WeatherStationId = stations[0].Id },
                new Location { Name = "tartu", WeatherStationId = stations[1].Id },
                new Location { Name = "pärnu", WeatherStationId = stations[2].Id }
            };
            DbContext.Locations.AddRange(locations);
            DbContext.SaveChanges();

            // Seed vehicle types
            var vehicleTypes = new[]
            {
                new VehicleType { Name = "car" },
                new VehicleType { Name = "scooter" },
                new VehicleType { Name = "bike" }
            };
            DbContext.VehicleTypes.AddRange(vehicleTypes);
            DbContext.SaveChanges();

            // Seed fee types
            var feeTypes = new[]
            {
                new FeeType { Code = Constants.Fees.Rbf, Name = "Regional Base Fee" },
                new FeeType { Code = "wief", Name = "Weather-dependent Extra Fee" },
                new FeeType { Code = "atef", Name = "Air Temperature Extra Fee" }
            };
            DbContext.FeeTypes.AddRange(feeTypes);
            DbContext.SaveChanges();

            // Seed condition types (severity grades)
            var conditionTypes = new[]
            {
                new ConditionType { Grade = 0 },
                new ConditionType { Grade = 1 },
                new ConditionType { Grade = 2 },
                new ConditionType { Grade = 3 }
            };
            DbContext.ConditionTypes.AddRange(conditionTypes);
            DbContext.SaveChanges();

            // Seed conditions
            var conditions = new[]
            {
                new WeatherCondition { Name = "clear", ConditionTypeId = conditionTypes[0].Id },
                new WeatherCondition { Name = "few clouds", ConditionTypeId = conditionTypes[0].Id },
                new WeatherCondition { Name = "variable clouds", ConditionTypeId = conditionTypes[0].Id },
                new WeatherCondition { Name = "cloudy with clear spells", ConditionTypeId = conditionTypes[0].Id },
                new WeatherCondition { Name = "overcast", ConditionTypeId = conditionTypes[0].Id },
                new WeatherCondition { Name = "light snow shower", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "moderate snow shower", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "heavy snow shower", ConditionTypeId = conditionTypes[2].Id },
                new WeatherCondition { Name = "light shower", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "moderate shower", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "heavy shower", ConditionTypeId = conditionTypes[2].Id },
                new WeatherCondition { Name = "light rain", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "moderate rain", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "heavy rain", ConditionTypeId = conditionTypes[2].Id },
                new WeatherCondition { Name = "light snowfall", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "moderate snowfall", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "heavy snowfall", ConditionTypeId = conditionTypes[2].Id },
                new WeatherCondition { Name = "snowstorm", ConditionTypeId = conditionTypes[3].Id },
                new WeatherCondition { Name = "drifting snow", ConditionTypeId = conditionTypes[2].Id },
                new WeatherCondition { Name = "hail", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "mist", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "fog", ConditionTypeId = conditionTypes[1].Id },
                new WeatherCondition { Name = "thunder", ConditionTypeId = conditionTypes[2].Id },
                new WeatherCondition { Name = "thunderstorm", ConditionTypeId = conditionTypes[3].Id }
            };
            DbContext.WeatherConditions.AddRange(conditions);
            DbContext.SaveChanges();

            // Seed fees
            var fees = new[]
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
            DbContext.Fees.AddRange(fees);
            DbContext.SaveChanges();

            // Seed weather forecasts (with current datetime)
            var forecasts = new[]
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
            DbContext.WeatherForecasts.AddRange(forecasts);
            DbContext.SaveChanges();
        }

        /// <summary>
        /// Seeds the provided database connection with test data
        /// </summary>
        public static void SeedTestData(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var db = new AppDbContext(options))
            {
                // Seed weather stations
                var stations = new[]
                {
                    new WeatherStation { Name = "Tallinn-Harku", WmoCode = 26038 },
                    new WeatherStation { Name = "Tartu-Tõravere", WmoCode = 26242 },
                    new WeatherStation { Name = "Pärnu", WmoCode = 41803 }
                };
                db.WeatherStations.AddRange(stations);
                db.SaveChanges();

                // Seed locations
                var locations = new[]
                {
                    new Location { Name = "tallinn", WeatherStationId = stations[0].Id },
                    new Location { Name = "tartu", WeatherStationId = stations[1].Id },
                    new Location { Name = "pärnu", WeatherStationId = stations[2].Id }
                };
                db.Locations.AddRange(locations);
                db.SaveChanges();

                // Seed vehicle types
                var vehicleTypes = new[]
                {
                    new VehicleType { Name = "car" },
                    new VehicleType { Name = "scooter" },
                    new VehicleType { Name = "bike" }
                };
                db.VehicleTypes.AddRange(vehicleTypes);
                db.SaveChanges();

                // Seed fee types
                var feeTypes = new[]
                {
                    new FeeType { Code = Constants.Fees.Rbf, Name = "Regional Base Fee" },
                    new FeeType { Code = "wief", Name = "Weather-dependent Extra Fee" },
                    new FeeType { Code = "atef", Name = "Air Temperature Extra Fee" }
                };
                db.FeeTypes.AddRange(feeTypes);
                db.SaveChanges();

                // Seed condition types (severity grades)
                var conditionTypes = new[]
                {
                    new ConditionType { Grade = 0 },
                    new ConditionType { Grade = 1 },
                    new ConditionType { Grade = 2 },
                    new ConditionType { Grade = 3 }
                };
                db.ConditionTypes.AddRange(conditionTypes);
                db.SaveChanges();

                // Seed conditions (abbreviated for brevity)
                var conditions = new[]
                {
                    new WeatherCondition { Name = "clear", ConditionTypeId = conditionTypes[0].Id },
                    new WeatherCondition { Name = "few clouds", ConditionTypeId = conditionTypes[0].Id },
                    new WeatherCondition { Name = "variable clouds", ConditionTypeId = conditionTypes[0].Id },
                    new WeatherCondition { Name = "light rain", ConditionTypeId = conditionTypes[1].Id },
                    new WeatherCondition { Name = "moderate rain", ConditionTypeId = conditionTypes[1].Id },
                    new WeatherCondition { Name = "heavy rain", ConditionTypeId = conditionTypes[2].Id },
                    new WeatherCondition { Name = "thunderstorm", ConditionTypeId = conditionTypes[3].Id }
                };
                db.WeatherConditions.AddRange(conditions);
                db.SaveChanges();

                // Seed regional base fees (abbreviated)
                var fees = new[]
                {
                    // Tallinn fees
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[0].Id, VehicleTypeId = vehicleTypes[0].Id, Amount = 4.0 },
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[0].Id, VehicleTypeId = vehicleTypes[1].Id, Amount = 3.5 },
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[0].Id, VehicleTypeId = vehicleTypes[2].Id, Amount = 3.0 },
                    
                    // Tartu fees
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[1].Id, VehicleTypeId = vehicleTypes[0].Id, Amount = 3.5 },
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[1].Id, VehicleTypeId = vehicleTypes[1].Id, Amount = 3.0 },
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[1].Id, VehicleTypeId = vehicleTypes[2].Id, Amount = 2.5 },
                    
                    // Pärnu fees
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[2].Id, VehicleTypeId = vehicleTypes[0].Id, Amount = 3.0 },
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[2].Id, VehicleTypeId = vehicleTypes[1].Id, Amount = 2.5 },
                    new Fee { FeeTypeId = feeTypes[0].Id, WeatherStationId = stations[2].Id, VehicleTypeId = vehicleTypes[2].Id, Amount = 2.0 }
                };
                db.Fees.AddRange(fees);
                db.SaveChanges();

                // Seed weather forecasts (with current datetime)
                var forecasts = new[]
                {
                    new WeatherForecast { WeatherStationId = stations[0].Id, DateTime = DateTime.UtcNow, AirTemperature = 5, WindSpeed = 5, Phenomenon = "clear" },
                    new WeatherForecast { WeatherStationId = stations[1].Id, DateTime = DateTime.UtcNow, AirTemperature = 5, WindSpeed = 4, Phenomenon = "few clouds" },
                    new WeatherForecast { WeatherStationId = stations[2].Id, DateTime = DateTime.UtcNow, AirTemperature = 6, WindSpeed = 3, Phenomenon = "variable clouds" }
                };
                db.WeatherForecasts.AddRange(forecasts);
                db.SaveChanges();
            }
        }

        public void Dispose()
        {
            DbContext?.Dispose();
            Connection?.Dispose();
        }
    }
} 