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

var builder = WebApplication.CreateBuilder(args);

//Adds services for using Problem Details format
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

// Add services to the container.

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1,0);
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }
).AddMvc();

builder.Services.AddControllers();

// Register HttpClient for WeatherJob
builder.Services.AddHttpClient();

var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection");

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(hangfireConnectionString));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

// Add framework services.
builder.Services.AddMvc();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHangfireDashboard();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Returns the Problem Details response for (empty) non-successful responses
app.UseStatusCodePages();

app.UseExceptionHandler();

app.Services.GetRequiredService<IRecurringJobManager>()
    .AddOrUpdate<WeatherJob>(
        "weather-job", 
        x => x.GetWeatherDataAsync(), 
        app.Configuration["Hangfire:CronSchedule"] ?? "15 * * * *");
app.Run();