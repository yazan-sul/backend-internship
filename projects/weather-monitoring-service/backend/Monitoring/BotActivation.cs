namespace WeatherMonitoringService.Monitoring;

public sealed record BotActivation
{
    public BotActivation(string bot, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bot);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Bot = bot;
        Message = message;
    }

    public string Bot { get; }

    public string Message { get; }
}
