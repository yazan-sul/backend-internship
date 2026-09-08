using WeatherMonitoringService.Configuration;

namespace WeatherMonitoringService;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddWeatherBotOptions(builder.Configuration);

        var app = builder.Build();

        app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

        app.Run();
    }
}
