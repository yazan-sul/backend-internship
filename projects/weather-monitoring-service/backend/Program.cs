using WeatherMonitoringService.Configuration;
using WeatherMonitoringService.Bots;
using WeatherMonitoringService.Monitoring;
using WeatherMonitoringService.Parsing;

namespace WeatherMonitoringService;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddWeatherBotOptions(builder.Configuration);
        builder.Services.AddSingleton<IWeatherDataParser, JsonWeatherDataParser>();
        builder.Services.AddSingleton<IWeatherDataParser, XmlWeatherDataParser>();
        builder.Services.AddSingleton<WeatherDataParserCoordinator>();
        builder.Services.AddSingleton<IWeatherObserver, RainBot>();
        builder.Services.AddSingleton<IWeatherObserver, SunBot>();
        builder.Services.AddSingleton<IWeatherObserver, SnowBot>();
        builder.Services.AddSingleton<WeatherMonitor>();

        var app = builder.Build();

        app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

        app.Run();
    }
}
