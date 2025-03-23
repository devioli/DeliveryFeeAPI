using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Persistence.Models.Fee;

public class FeeType
{
    // Regional base fee, air temp, wind speed fee etc.
    public Guid Id { get; set; }
    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }
    [Required]
    [MaxLength(50)]
    public required string Code { get; set; }
    
    public ICollection<Fee>? Fees { get; set; }
}