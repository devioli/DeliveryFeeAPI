using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Models.Weather.Condition;

public class WeatherCondition
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    public Guid ConditionTypeId { get; set; }
}