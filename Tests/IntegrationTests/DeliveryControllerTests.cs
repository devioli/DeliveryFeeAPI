using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using App.DTOs.v1;
using FluentAssertions;
using Xunit;
using static Domain.Constants.Constants;

namespace Tests.IntegrationTests;

[Collection("DeliveryApiTests")]
public class DeliveryControllerTests(CustomWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string BaseUrl = "api/v1/delivery";

    #region Path Parameter Tests

    [Fact]
    public async Task GetDeliveryFee_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var delivery = CreateDeliveryDto();

        // Act
        var response = await _client.GetAsync(BaseUrl + $"/{delivery.City}/{delivery.VehicleType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        
        var result = await response.Content.ReadFromJsonAsync<DeliveryDTOResponse>();
        result.Should().NotBeNull();
        result.Should().BeOfType<DeliveryDTOResponse>();
        result!.Fee.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDeliveryFee_WithTimestamp_ReturnsOkResult()
    {
        // Arrange
        var time = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
        var delivery = CreateDeliveryDto(timestamp: time);

        // Act
        var response = await _client.GetAsync(BaseUrl + $"/{delivery.City}/{delivery.VehicleType}?timestamp={delivery.Timestamp}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        
        var result = await response.Content.ReadFromJsonAsync<DeliveryDTOResponse>();
        result.Should().NotBeNull();
        result.Should().BeOfType<DeliveryDTOResponse>();
        result!.Fee.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDeliveryFee_WithInvalidCity_ReturnsNotFound()
    {
        // Arrange
        var delivery = CreateDeliveryDto(city: "Kuressaare");

        // Act
        var response = await _client.GetAsync(BaseUrl + $"/{delivery.City}/{delivery.VehicleType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain($"Weather station for location '{delivery.City.ToLowerInvariant()}' was not found");
    }

    [Fact]
    public async Task GetDeliveryFee_WithInvalidVehicleType_ReturnsNotFound()
    {
        // Arrange
        var delivery = CreateDeliveryDto(vehicleType: "Bus");

        // Act
        var response = await _client.GetAsync(BaseUrl + $"/{delivery.City}/{delivery.VehicleType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain($"Vehicle type '{delivery.VehicleType.ToLowerInvariant()}' was not found");
    }

    #endregion

    #region Query Parameter Tests

    [Fact]
    public async Task GetDeliveryFeeFromQuery_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var delivery = CreateDeliveryDto();

        // Act
        var response = await _client.GetAsync(BaseUrl + $"?city={delivery.City}&vehicleType={delivery.VehicleType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        
        var result = await response.Content.ReadFromJsonAsync<DeliveryDTOResponse>();
        result.Should().NotBeNull();
        result.Should().BeOfType<DeliveryDTOResponse>();
        result!.Fee.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDeliveryFeeFromQuery_WithTimestamp_ReturnsOkResult()
    {
        // Arrange
        var time = DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds();
        var delivery = CreateDeliveryDto(timestamp: time);

        // Act
        var response = await _client.GetAsync(BaseUrl + $"?city={delivery.City}&vehicleType={delivery.VehicleType}&timestamp={delivery.Timestamp}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        
        var result = await response.Content.ReadFromJsonAsync<DeliveryDTOResponse>();
        result.Should().NotBeNull();
        result.Should().BeOfType<DeliveryDTOResponse>();
        result!.Fee.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDeliveryFeeFromQuery_WithInvalidCity_ReturnsNotFound()
    {
        // Arrange
        var delivery = CreateDeliveryDto(city: "Kuressaare");

        // Act
        var response = await _client.GetAsync(BaseUrl + $"?city={delivery.City}&vehicleType={delivery.VehicleType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain($"Weather station for location '{delivery.City.ToLowerInvariant()}' was not found");
    }

    [Fact]
    public async Task GetDeliveryFeeFromQuery_WithInvalidVehicleType_ReturnsNotFound()
    {
        // Arrange
        var delivery = CreateDeliveryDto(vehicleType: "Bus");

        // Act
        var response = await _client.GetAsync(BaseUrl + $"?city={delivery.City}&vehicleType={delivery.VehicleType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("detail").GetString().Should().Contain($"Vehicle type '{delivery.VehicleType.ToLowerInvariant()}' was not found");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task GetDeliveryFeeFromQuery_WithMissingParameters_ReturnsBadRequest()
    {
        // Arrange & Act
        var response = await _client.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.TryGetProperty("errors", out var _).Should().BeTrue();
    }
    
    #endregion
    
    #region Data-Driven Tests
    
    [Theory]
    [InlineData(Locations.Tallinn, Vehicles.Car)]
    [InlineData(Locations.Tallinn, Vehicles.Scooter)]
    [InlineData(Locations.Tallinn, Vehicles.Bike)]
    [InlineData(Locations.Tartu, Vehicles.Car)]
    [InlineData(Locations.Tartu, Vehicles.Scooter)]
    [InlineData(Locations.Tartu, Vehicles.Bike)]
    [InlineData(Locations.Pärnu, Vehicles.Car)]
    [InlineData(Locations.Pärnu, Vehicles.Scooter)]
    [InlineData(Locations.Pärnu, Vehicles.Bike)]
    public async Task GetDeliveryFee_ShouldReturnSuccess_ForAllValidCombinations(string city, string vehicleType)
    {
        // Arrange
        var delivery = CreateDeliveryDto(city: city, vehicleType: vehicleType);

        // Act
        var response = await _client.GetAsync(BaseUrl + $"/{delivery.City}/{delivery.VehicleType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<DeliveryDTOResponse>();
        result.Should().NotBeNull();
        result!.Fee.Should().BeGreaterThan(0);
    }
    
    #endregion
    
    #region Test Helpers

    private DeliveryDTO CreateDeliveryDto(string? city = Locations.Tallinn, string? vehicleType = Vehicles.Car, long? timestamp = null)
    {
        return new DeliveryDTO
        {
            City = city!,
            VehicleType = vehicleType!,
            Timestamp = timestamp
        };
    }
    
    #endregion
} 