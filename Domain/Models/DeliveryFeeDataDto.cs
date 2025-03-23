namespace Domain.Models;

public class DeliveryFeeDataDto
{
    public Guid? StationId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? FeeTypeId { get; set; }
    public double RegionalBaseFee { get; set; }
    public WeatherForecastDto? WeatherForecast { get; set; }
    public IEnumerable<WeatherConditionDto> WeatherConditions { get; set; } = new List<WeatherConditionDto>();
}