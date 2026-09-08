using Microsoft.Extensions.Options;
using WeatherMonitoringService.Configuration;
using WeatherMonitoringService.Monitoring;
using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Bots;

public sealed class SnowBot(IOptions<SnowBotOptions> options) : IWeatherObserver
{
    private readonly SnowBotOptions _options = options.Value;

    public BotActivation? Notify(WeatherData weather)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return _options.Enabled is true && weather.Temperature < _options.TemperatureThreshold
            ? new BotActivation(nameof(SnowBot), _options.Message!)
            : null;
    }
}
