namespace Infrastructure.Persistence.Models;

public class ConditionType
{
    public Guid Id { get; set; }
    public int Grade { get; set; } = 1;
    public ICollection<WeatherCondition>? WeatherConditions { get; set; }
}