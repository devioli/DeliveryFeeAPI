using System.Xml.Linq;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models.Weather.Forecast;

namespace Infrastructure.BackgroundJobs;

public class WeatherJob (AppDbContext dbContext, IRepository repository)
{
    public async Task FetchWeatherDataAsync()
    {
        await GetData();
        await dbContext.SaveChangesAsync();
    }
    
    public async Task GetData()
    {
        // URL of the XML data
        var url = "https://www.ilmateenistus.ee/ilma_andmed/xml/observations.php";

        // Use HttpClient to fetch the XML data
        using var client = new HttpClient();
        try
        {
            // Download the XML as a string
            var xmlData = await client.GetStringAsync(url);

            // Parse the XML string into an XDocument
            var doc = XDocument.Parse(xmlData);

            // Get weather stations from repository
            var stations = await repository.GetAllWeatherStationsAsync();

            // Query the XML for stations matching the target names
            var weatherForecasts = doc.Descendants("station")
                .Where(s =>
                {
                    var nameElement = s.Element("name");
                    return nameElement != null && stations.Select(x => x.Name).Contains(nameElement.Value.ToLower());
                })
                .Select(s =>
                {
                    var stationName = s.Element("name")?.Value?.ToLower() ?? "";
                    var matchingStation = stations.First(x => x.Name.ToLower() == stationName);
                    return new WeatherForecast
                    {
                        WeatherStationId = matchingStation.Id,
                        AirTemperature = double.Parse(s.Element("airtemperature")?.Value ?? "0"),
                        WindSpeed = double.Parse(s.Element("windspeed")?.Value ?? "0"),
                        Phenomenon = s.Element("phenomenon")?.Value?.ToLower() ?? "",
                        DateTime = UnixSecondsToDateTime(long.Parse(doc.Root?.Attribute("timestamp")?.Value ?? "0")),
                    };
                });

            await dbContext.WeatherForecasts.AddRangeAsync(weatherForecasts);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public DateTime UnixSecondsToDateTime(long timestamp, bool local = false)
    {
        var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return local ? offset.LocalDateTime : offset.UtcDateTime;
    }
}