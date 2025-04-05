using App;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.MemoryStorage;
using Moq;
using Xunit;

namespace Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the app's database context registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a database context using SQLite in-memory mode for testing
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Mock Hangfire services to prevent database access
            var bgJobClientDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IBackgroundJobClient));
            if (bgJobClientDescriptor != null)
            {
                services.Remove(bgJobClientDescriptor);
            }
            
            var recurringJobManagerDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IRecurringJobManager));
            if (recurringJobManagerDescriptor != null)
            {
                services.Remove(recurringJobManagerDescriptor);
            }

            // Add mock implementations
            var mockJobClient = new Mock<IBackgroundJobClient>();
            mockJobClient.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
                .Returns("job-id");
            services.AddSingleton(mockJobClient.Object);

            var mockJobManager = new Mock<IRecurringJobManager>();
            services.AddSingleton(mockJobManager.Object);

            // Use Hangfire's in-memory storage for testing
            services.AddHangfire(configuration => configuration.UseMemoryStorage());

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}

/// <summary>
/// Collection definition for sharing the WebApplicationFactory across tests
/// </summary>
[CollectionDefinition("DeliveryApiTests")]
public class SharedTestCollection : ICollectionFixture<CustomWebApplicationFactory>; 