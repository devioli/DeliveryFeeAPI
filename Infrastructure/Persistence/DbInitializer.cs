using Infrastructure.BackgroundJobs;
using Infrastructure.Persistence.Models.Fee;
using Infrastructure.Persistence.Models.Weather.Station;
using Infrastructure.Persistence.Models.Vehicle;
using Infrastructure.Persistence.Models.Weather.Condition;
using Microsoft.EntityFrameworkCore;
using static Domain.Constants.Constants;

namespace Infrastructure.Persistence;

public class DbInitializer
{
    // Define constants for reference data IDs to improve readability
    private static class Ids
    {
        // Vehicle types
        public static readonly Guid Car = Guid.Parse("4F0DD1C3-0C19-4EF9-A244-5DD246B9E766");
        public static readonly Guid Scooter = Guid.Parse("6070D6D0-4DD0-445C-B68F-B4F18AEFDE27");
        public static readonly Guid Bike = Guid.Parse("80D0E398-642B-4253-80CF-610680219E26");
        
        // Stations
        public static readonly Guid StationTallinn = Guid.Parse("77B57BC0-B9CE-4A06-B9CF-92A55C532579");
        public static readonly Guid StationTartu = Guid.Parse("36430807-3F8F-4C8E-A69E-EF2ACED338DF");
        public static readonly Guid StationParnu = Guid.Parse("BEF307A8-4857-44C6-A017-E5F2D5676372");
        
        // Locations
        public static readonly Guid LocationTallinn = Guid.Parse("353C458F-A964-442A-A995-A9440ECF3B99");
        public static readonly Guid LocationTartu = Guid.Parse("B8CE05FC-A05C-46F0-A331-73DA9CBCA379");
        public static readonly Guid LocationParnu = Guid.Parse("88E6B7CD-B087-410A-932D-8EBF329BC39D");
        
        // Fee types
        public static readonly Guid RegionalBaseFee = Guid.Parse("F65FAA88-B1A3-4947-9199-7E86D7B93951");
        
        // Weather condition types
        public static readonly Guid ConditionTypeGrade1 = Guid.Parse("CA170A46-50A9-410D-8115-C48048CDE78A");
        public static readonly Guid ConditionTypeGrade2 = Guid.Parse("75E70940-192F-4893-88F2-2B07FA0CA0AB");
        public static readonly Guid ConditionTypeGrade3 = Guid.Parse("B58E42DB-5B7D-47C6-AE54-878379B65DBE");
    }

    private readonly AppDbContext _context;
    private readonly WeatherJob _weatherJob;

    public DbInitializer(AppDbContext context, WeatherJob weatherJob)
    {
        _context = context;
        _weatherJob = weatherJob;
    }

    public async Task InitializeAsync()
    {
        if (_context.Database.IsSqlite())
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();
        }

        // Only seed if there are no vehicle types (as an indicator that the DB is empty)
        if (!await _context.Set<VehicleType>().AnyAsync())
        {
            await SeedDataAsync();
            await _weatherJob.GetWeatherDataAsync();
        }
    }

    private async Task SeedDataAsync()
    {
        // Vehicle types
        var car = new VehicleType { Id = Ids.Car, Name = Vehicles.Car };
        var scooter = new VehicleType { Id = Ids.Scooter, Name = Vehicles.Scooter };
        var bike = new VehicleType { Id = Ids.Bike, Name = Vehicles.Bike };
        
        await _context.Set<VehicleType>().AddRangeAsync(car, scooter, bike);

        // Weather stations
        var stationTallinn = new WeatherStation { Id = Ids.StationTallinn, Name = Stations.Tallinn, WmoCode = 26038 };
        var stationTartu = new WeatherStation { Id = Ids.StationTartu, Name = Stations.Tartu, WmoCode = 26242 };
        var stationParnu = new WeatherStation { Id = Ids.StationParnu, Name = Stations.Pärnu, WmoCode = 41803 };
        
        await _context.Set<WeatherStation>().AddRangeAsync(stationTallinn, stationTartu, stationParnu);
        
        // Locations
        var tallinn = new Location { Id = Ids.LocationTallinn, Name = Locations.Tallinn, WeatherStationId = stationTallinn.Id };
        var tartu = new Location { Id = Ids.LocationTartu, Name = Locations.Tartu, WeatherStationId = stationTartu.Id };
        var parnu = new Location { Id = Ids.LocationParnu, Name = Locations.Pärnu, WeatherStationId = stationParnu.Id };
        
        await _context.Set<Location>().AddRangeAsync(tallinn, tartu, parnu);
        
        // Fee type
        var rbf = new FeeType { Id = Ids.RegionalBaseFee, Name = Fees.RegionalBaseFee, Code = Fees.Rbf };
        
        await _context.Set<FeeType>().AddAsync(rbf);
        
        // Fees - using Guid.NewGuid() for entities that don't need predefined IDs
        var fees = new List<Fee>
        {
            new() { Id = Guid.NewGuid(), WeatherStationId = stationTallinn.Id, VehicleTypeId = car.Id, FeeTypeId = rbf.Id, Amount = 4 },
            new() { Id = Guid.NewGuid(), WeatherStationId = stationTartu.Id, VehicleTypeId = car.Id, FeeTypeId = rbf.Id, Amount = 3.5 },
            new() { Id = Guid.NewGuid(), WeatherStationId = stationParnu.Id, VehicleTypeId = car.Id, FeeTypeId = rbf.Id, Amount = 3 },
            
            new() { Id = Guid.NewGuid(), WeatherStationId = stationTallinn.Id, VehicleTypeId = scooter.Id, FeeTypeId = rbf.Id, Amount = 3.5 },
            new() { Id = Guid.NewGuid(), WeatherStationId = stationTartu.Id, VehicleTypeId = scooter.Id, FeeTypeId = rbf.Id, Amount = 3 },
            new() { Id = Guid.NewGuid(), WeatherStationId = stationParnu.Id, VehicleTypeId = scooter.Id, FeeTypeId = rbf.Id, Amount = 2.5 },
            
            new() { Id = Guid.NewGuid(), WeatherStationId = stationTallinn.Id, VehicleTypeId = bike.Id, FeeTypeId = rbf.Id, Amount = 3 },
            new() { Id = Guid.NewGuid(), WeatherStationId = stationTartu.Id, VehicleTypeId = bike.Id, FeeTypeId = rbf.Id, Amount = 2.5 },
            new() { Id = Guid.NewGuid(), WeatherStationId = stationParnu.Id, VehicleTypeId = bike.Id, FeeTypeId = rbf.Id, Amount = 2 }
        };
        
        await _context.Set<Fee>().AddRangeAsync(fees);

        // Weather condition types
        var categoryOne = new ConditionType { Id = Ids.ConditionTypeGrade1, Grade = 1 };
        var categoryTwo = new ConditionType { Id = Ids.ConditionTypeGrade2, Grade = 2 };
        var categoryThree = new ConditionType { Id = Ids.ConditionTypeGrade3, Grade = 3 };

        await _context.Set<ConditionType>().AddRangeAsync(categoryOne, categoryTwo, categoryThree);

        // Weather conditions - using Guid.NewGuid() since these don't need predefined IDs
        var conditions = new List<WeatherCondition>
        {
            new() { Id = Guid.NewGuid(), Name = "glaze", ConditionTypeId = categoryThree.Id },
            new() { Id = Guid.NewGuid(), Name = "hail", ConditionTypeId = categoryThree.Id },
            new() { Id = Guid.NewGuid(), Name = "thunder", ConditionTypeId = categoryThree.Id },
            
            new() { Id = Guid.NewGuid(), Name = "snow", ConditionTypeId = categoryTwo.Id },
            new() { Id = Guid.NewGuid(), Name = "sleet", ConditionTypeId = categoryTwo.Id },
            
            new() { Id = Guid.NewGuid(), Name = "rain", ConditionTypeId = categoryOne.Id }
        };
        
        await _context.Set<WeatherCondition>().AddRangeAsync(conditions);
        
        // Save all changes
        await _context.SaveChangesAsync();
    }
} 