namespace WeatherMonitoringService.Configuration;

public abstract class WeatherBotOptions
{
    public bool? Enabled { get; init; }

    public string? Message { get; init; }
}

public sealed class RainBotOptions : WeatherBotOptions
{
    public const string SectionName = "RainBot";

    public double? HumidityThreshold { get; init; }
}

public sealed class SunBotOptions : WeatherBotOptions
{
    public const string SectionName = "SunBot";

    public double? TemperatureThreshold { get; init; }
}

public sealed class SnowBotOptions : WeatherBotOptions
{
    public const string SectionName = "SnowBot";

    public double? TemperatureThreshold { get; init; }
}
