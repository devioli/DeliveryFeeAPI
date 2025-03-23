namespace Domain.Models;

public class WeatherForecastDto
{
    public Guid Id { get; set; }
    public double AirTemperature { get; set; }
    public double WindSpeed { get; set; }
    public string? Phenomenon { get; set; }
    public DateTime DateTime { get; set; }
    public Guid WeatherStationId { get; set; }
} 