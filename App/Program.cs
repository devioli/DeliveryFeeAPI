using Asp.Versioning;
using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Persistence;
using App.Middleware;
using Hangfire;
using Hangfire.Storage.SQLite;
using Infrastructure.BackgroundJobs;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi.Models;
using App.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add service for using Problem Details format
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Type = null;
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

// Hybrid Cache
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024;
    options.MaximumKeyLength = 1024;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});

// Add custom exception handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

// Add services to the container.

// Add API versioning support
builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1,0);
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }
);

// Enable OpenAPI/Swagger with Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Delivery Fee API", 
        Version = "v1",
        Description = "API for calculating delivery fees based on weather conditions."
    });
});

// Register HttpClient for WeatherJob
builder.Services.AddHttpClient();

// Add Hangfire services.
var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection");

builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(hangfireConnectionString));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IService, Service>();
builder.Services.AddScoped<WeatherJob>();

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await initializer.InitializeAsync();
}

app.UseExceptionHandler();

// HTTPS redirection should come early in the pipeline
app.UseHttpsRedirection();

// Enable Swagger for all environments
app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Delivery Fee API v1");
    });
}

app.UseHangfireDashboard();

// Returns the Problem Details response for (empty) non-successful responses
app.UseStatusCodePages();

// Map all API endpoints
app.MapEndpoints();

app.Services.GetRequiredService<IRecurringJobManager>()
    .AddOrUpdate<WeatherJob>(
        "weather-job", 
        x => x.GetWeatherDataAsync(), 
        app.Configuration["Hangfire:CronSchedule"] ?? "15 * * * *");
app.Run();