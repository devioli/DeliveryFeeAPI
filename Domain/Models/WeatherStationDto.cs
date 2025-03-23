namespace Domain.Models;

public class WeatherStationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int WmoCode { get; set; }
} 