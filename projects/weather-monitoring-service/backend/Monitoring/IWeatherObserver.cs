using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Monitoring;

public interface IWeatherObserver
{
    BotActivation? Notify(WeatherData weather);
}
