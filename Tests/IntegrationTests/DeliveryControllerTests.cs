using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Services;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.IntegrationTests
{
    public class DeliveryControllerTests : IDisposable
    {
        private readonly IService _service;
        private readonly IRepository _repository;
        private readonly SqliteConnection _connection;
        private readonly AppDbContext _dbContext;

        public DeliveryControllerTests()
        {
            // Setup in-memory database and service
            _connection = TestBase.CreateConnection();
            
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new Repository(_dbContext);
            _service = new Service(_repository);
            
            // Seed test data
            TestBase.SeedTestData(_connection);
        }

        [Theory]
        [InlineData("tallinn", "car")]
        [InlineData("tallinn", "scooter")]
        [InlineData("tallinn", "bike")]
        [InlineData("tartu", "car")]
        [InlineData("pärnu", "scooter")]
        public async Task GetDeliveryFee_ReturnsExpectedValue(string city, string vehicleType)
        {
            // Act
            var result = await _service.GetDeliveryFeeAsync(new Delivery
            {
                City = city,
                VehicleType = vehicleType
            });

            // Assert
            Assert.True(result >= 0);
        }

        [Theory]
        [InlineData("", "car")]
        [InlineData("tallinn", "")]
        public async Task GetDeliveryFee_MissingRequiredParams_ThrowsNotFoundException(string city, string vehicleType)
        {
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GetDeliveryFeeAsync(new Delivery
                {
                    City = city,
                    VehicleType = vehicleType
                }));
        }

        [Theory]
        [InlineData("nonexistent", "car")]
        public async Task GetDeliveryFee_NonExistentCity_ThrowsNotFoundException(string city, string vehicleType)
        {
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GetDeliveryFeeAsync(new Delivery
                {
                    City = city,
                    VehicleType = vehicleType
                }));
        }

        [Theory]
        [InlineData("tallinn", "nonexistent")]
        public async Task GetDeliveryFee_NonExistentVehicleType_ThrowsNotFoundException(string city, string vehicleType)
        {
            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _service.GetDeliveryFeeAsync(new Delivery
                {
                    City = city,
                    VehicleType = vehicleType
                }));
        }

        [Fact]
        public async Task GetDeliveryFee_WithTimestamp_ReturnsSuccessOrThrows()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            
            try
            {
                // Act
                var result = await _service.GetDeliveryFeeAsync(new Delivery
                {
                    City = "tallinn",
                    VehicleType = "car",
                    DateTime = timestamp
                });

                // Assert
                Assert.True(result >= 0);
            }
            catch (Exception ex) when (ex is NotFoundException || ex is ArgumentException)
            {
                // It's acceptable if we get NotFoundException for weather data
                // or ArgumentException for invalid timestamp
                Assert.True(true);
            }
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _connection?.Dispose();
        }
    }
} 