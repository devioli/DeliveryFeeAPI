using System.Globalization;
using System.Xml.Linq;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.BackgroundJobs;

public class WeatherJob (AppDbContext dbContext, IRepository repository, IConfiguration configuration, HybridCache hybridCache)
{
    public async Task GetWeatherDataAsync()
    {
        var xml = await FetchXmlDataAsync();
        await XmlToWeatherForecastMapperAsync(xml);
        await dbContext.SaveChangesAsync();
        await hybridCache.RemoveAsync("weather");
    }

    private async Task<string> FetchXmlDataAsync()
    {
        var url = configuration["WEATHER_API_URL"] ?? throw new Exception("WEATHER_API_URL variable is not set.");
        using var client = new HttpClient();
        try
        {
            return await client.GetStringAsync(url);
        }
        catch (Exception exception)
        {
            throw new Exception($"Failed to fetch weather data: {exception.Message}");
        }
    }

    private async Task XmlToWeatherForecastMapperAsync(string xml)
    {
        var doc = XDocument.Parse(xml);
        var stations = await repository.GetAllWeatherStationsAsync();
        var weatherForecasts = doc.Descendants("station")
            .Where(s =>
            {
                var nameElement = s.Element("name");
                return nameElement != null && stations.Select(x => x.Name).Contains(nameElement.Value.ToLower());
            })
            .Select(s =>
            {
                var stationName = s.Element("name")?.Value.ToLower() ?? "";
                var matchingStation = stations.First(x => x.Name.Equals(stationName, StringComparison.CurrentCultureIgnoreCase));
                
                return new WeatherForecast
                {
                    WeatherStationId = matchingStation.Id,
                    AirTemperature = double.Parse(s.Element("airtemperature")?.Value ?? "0", CultureInfo.InvariantCulture),
                    WindSpeed = double.Parse(s.Element("windspeed")?.Value ?? "0", CultureInfo.InvariantCulture),
                    Phenomenon = s.Element("phenomenon")?.Value.ToLower() ?? "",
                    DateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(doc.Root?.Attribute("timestamp")?.Value ?? "0")).DateTime
                };
            });
        await dbContext.WeatherForecasts.AddRangeAsync(weatherForecasts);
    }
}