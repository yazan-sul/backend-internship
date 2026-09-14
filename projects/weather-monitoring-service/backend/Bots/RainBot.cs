using Microsoft.Extensions.Options;
using WeatherMonitoringService.Configuration;
using WeatherMonitoringService.Monitoring;
using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Bots;

public sealed class RainBot(IOptions<RainBotOptions> options) : IWeatherObserver
{
    private readonly RainBotOptions _options = options.Value;

    public BotActivation? Notify(WeatherData weather)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return _options.Enabled is true && weather.Humidity > _options.HumidityThreshold
            ? new BotActivation(nameof(RainBot), _options.Message!)
            : null;
    }
}
