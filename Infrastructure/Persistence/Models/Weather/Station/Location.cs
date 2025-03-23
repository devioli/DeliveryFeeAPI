using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Models.Weather.Station;

public class Location
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    [Required]
    public Guid WeatherStationId { get; set; }
    
}