using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Tests.Weather;

public sealed class WeatherDataTests
{
    [Fact]
    public void Constructor_CreatesWeatherData_WhenValuesAreValid()
    {
        var weather = new WeatherData("  Ramallah  ", 24.5, 60);

        Assert.Equal("Ramallah", weather.Location);
        Assert.Equal(24.5, weather.Temperature);
        Assert.Equal(60, weather.Humidity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankLocation(string location)
    {
        Assert.Throws<ArgumentException>(() => new WeatherData(location, 20, 50));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_RejectsNonFiniteTemperature(double temperature)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WeatherData("Ramallah", temperature, 50)
        );
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-0.1)]
    [InlineData(100.1)]
    public void Constructor_RejectsInvalidHumidity(double humidity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WeatherData("Ramallah", 20, humidity)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_AcceptsHumidityBoundary(double humidity)
    {
        var weather = new WeatherData("Ramallah", 20, humidity);

        Assert.Equal(humidity, weather.Humidity);
    }
}
