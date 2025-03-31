using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Models;

public class VehicleType
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    
    public ICollection<Fee>? Fees { get; set; }
}