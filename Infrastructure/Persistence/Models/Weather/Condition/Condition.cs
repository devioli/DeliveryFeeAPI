namespace Infrastructure.Persistence.Models.Weather.Condition;

public class Condition
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid ConditionTypeId { get; set; }
}