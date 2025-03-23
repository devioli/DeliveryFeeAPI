namespace Infrastructure.Persistence.Models.Fee;

public class Fee
{
    public Guid Id { get; set; }
    public Guid FeeTypeId { get; set; }
    public Guid VehicleTypeId { get; set; }
    public Guid WeatherStationId { get; set; }
    public double Amount { get; set; }
}