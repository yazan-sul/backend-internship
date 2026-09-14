using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Parsing;

public interface IWeatherDataParser
{
    bool CanHandle(string rawData);

    WeatherData Parse(string rawData);
}
