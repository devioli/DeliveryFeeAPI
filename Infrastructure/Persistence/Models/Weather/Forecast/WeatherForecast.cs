using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Models.Weather.Forecast;

public class WeatherForecast
{
    public Guid Id { get; set; }
    [Required]
    public double AirTemperature { get; set; }
    [Required]
    public double WindSpeed { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Phenomenon { get; set; }
    [Required]
    public DateTime DateTime { get; set; }
    
    [Required]
    public Guid WeatherStationId { get; set; }
}