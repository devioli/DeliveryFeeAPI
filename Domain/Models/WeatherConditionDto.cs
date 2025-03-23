namespace Domain.Models;


public class WeatherConditionDto
{
    public Guid Id { get; set; }
    public int Grade { get; set; }
    public ICollection<ConditionDto> Conditions { get; set; } = new List<ConditionDto>();
}