namespace WeatherMonitoringService.Weather;

public sealed record WeatherData
{
    public WeatherData(string location, double temperature, double humidity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);

        if (!double.IsFinite(temperature))
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature),
                temperature,
                "Temperature must be finite."
            );
        }

        if (!double.IsFinite(humidity) || humidity is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(humidity),
                humidity,
                "Humidity must be finite and between 0 and 100."
            );
        }

        Location = location.Trim();
        Temperature = temperature;
        Humidity = humidity;
    }

    public string Location { get; }

    public double Temperature { get; }

    public double Humidity { get; }
}
