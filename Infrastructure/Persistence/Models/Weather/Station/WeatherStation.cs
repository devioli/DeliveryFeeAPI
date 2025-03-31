using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Models;

public class WeatherStation
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    
    [Required]
    public int WmoCode { get; set; }
    
    public ICollection<Location>? Locations { get; set; }
    public ICollection<WeatherForecast>? Forecasts { get; set; }
}