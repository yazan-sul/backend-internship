using WeatherMonitoringService.Monitoring;
using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Tests.Monitoring;

public sealed class WeatherMonitorTests
{
    [Fact]
    public void Publish_ReturnsEveryActivation_InObserverRegistrationOrder()
    {
        var first = new StubObserver(new BotActivation("FirstBot", "First message"));
        var inactive = new StubObserver(null);
        var second = new StubObserver(new BotActivation("SecondBot", "Second message"));
        var monitor = new WeatherMonitor([first, inactive, second]);

        var activations = monitor.Publish(new WeatherData("Test City", 20, 50));

        Assert.Collection(
            activations,
            activation => Assert.Equal("FirstBot", activation.Bot),
            activation => Assert.Equal("SecondBot", activation.Bot)
        );
    }

    [Fact]
    public void Publish_ReturnsEmptyResult_WhenNoObserverActivates()
    {
        var monitor = new WeatherMonitor([new StubObserver(null)]);

        var activations = monitor.Publish(new WeatherData("Test City", 20, 50));

        Assert.Empty(activations);
    }

    private sealed class StubObserver(BotActivation? activation) : IWeatherObserver
    {
        public BotActivation? Notify(WeatherData weather) => activation;
    }
}
