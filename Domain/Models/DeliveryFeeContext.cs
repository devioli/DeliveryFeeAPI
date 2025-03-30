namespace Domain.Models;

public class DeliveryFeeContext
{
    public Guid? StationId { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? FeeTypeId { get; set; }
    public double RegionalBaseFee { get; set; }
    public int WeatherConditionGrade { get; set; }
    public ForecastDto? WeatherForecast { get; set; }
}