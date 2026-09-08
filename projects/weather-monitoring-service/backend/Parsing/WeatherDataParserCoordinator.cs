using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Parsing;

public sealed class WeatherDataParserCoordinator
{
    private readonly IReadOnlyList<IWeatherDataParser> _parsers;

    public WeatherDataParserCoordinator(IEnumerable<IWeatherDataParser> parsers)
    {
        ArgumentNullException.ThrowIfNull(parsers);
        _parsers = parsers.ToArray();
    }

    public WeatherData Parse(string? rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
        {
            throw new WeatherDataParsingException("Weather data cannot be empty.");
        }

        var parser = _parsers.FirstOrDefault(candidate => candidate.CanHandle(rawData));
        if (parser is null)
        {
            throw new WeatherDataParsingException(
                "Unsupported weather data format. Provide a JSON object or XML document."
            );
        }

        return parser.Parse(rawData);
    }
}
