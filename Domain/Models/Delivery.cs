namespace Domain.Models;

public class Delivery
{
    public required string City { get; set; }
    public required string VehicleType { get; set; }
    public DateTime? DateTime { get; set; }
}