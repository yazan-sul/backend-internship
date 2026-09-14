using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Monitoring;

public sealed class WeatherMonitor(IEnumerable<IWeatherObserver> observers)
{
    private readonly IReadOnlyList<IWeatherObserver> _observers = observers.ToArray();

    public IReadOnlyList<BotActivation> Publish(WeatherData weather)
    {
        ArgumentNullException.ThrowIfNull(weather);

        var activations = new List<BotActivation>();

        foreach (var observer in _observers)
        {
            var activation = observer.Notify(weather);

            if (activation is not null)
            {
                activations.Add(activation);
            }
        }

        return activations;
    }
}
