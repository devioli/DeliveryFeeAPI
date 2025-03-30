namespace Infrastructure.Persistence.Models.Weather.Condition;

public class ConditionType
{
    public Guid Id { get; set; }
    public int Grade { get; set; } = 1;
    public ICollection<WeatherCondition>? WeatherConditions { get; set; }
}