using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Models.Fee;
using Infrastructure.Persistence.Models.Weather.Station;
using Infrastructure.Persistence.Models.Vehicle;
using Infrastructure.Persistence.Models.Weather.Condition;
using Infrastructure.Persistence.Models.Weather.Forecast;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Fee> Fees { get; set; } = null!;
    public DbSet<FeeType> FeeTypes { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
    
    public DbSet<Condition> Conditions { get; set; } = null!;
    public DbSet<ConditionType> ConditionTypes { get; set; } = null!;
    public DbSet<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public DbSet<WeatherStation> WeatherStations { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var relationship in modelBuilder.Model
                     .GetEntityTypes()
                     .Where(e => !e.IsOwned())
                     .SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
    }
}