using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Models;
using Domain.Models;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Fee> Fees { get; set; } = null!;
    public DbSet<FeeType> FeeTypes { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<VehicleType> VehicleTypes { get; set; } = null!;
    public DbSet<WeatherCondition> WeatherConditions { get; set; } = null!;
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
        
        modelBuilder.Entity<VehicleType>()
            .HasIndex(v => v.Name)
            .IsUnique();
        
        modelBuilder.Entity<Location>()
            .HasIndex(l => l.Name)
            .IsUnique();
    }
}