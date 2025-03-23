namespace Domain.Models;

public class DeliveryFee
{
    public required string City { get; set; }
    public required string VehicleType { get; set; }
    
    public DateTime? DateTime { get; set; }
}