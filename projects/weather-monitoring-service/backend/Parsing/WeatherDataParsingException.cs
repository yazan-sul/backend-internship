namespace WeatherMonitoringService.Parsing;

public sealed class WeatherDataParsingException : Exception
{
    public WeatherDataParsingException(string message)
        : base(message) { }

    public WeatherDataParsingException(string message, Exception innerException)
        : base(message, innerException) { }
}
