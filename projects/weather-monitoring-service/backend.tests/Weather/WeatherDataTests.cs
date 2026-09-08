using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Tests.Weather;

public sealed class WeatherDataTests
{
    [Fact]
    public void Constructor_CreatesWeatherData_WhenValuesAreValid()
    {
        // Act
        var weather = new WeatherData("  Ramallah  ", 24.5, 60);

        // Assert
        Assert.Equal("Ramallah", weather.Location);
        Assert.Equal(24.5, weather.Temperature);
        Assert.Equal(60, weather.Humidity);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsBlankLocation(string location)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new WeatherData(location, 20, 50));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_RejectsNonFiniteTemperature(double temperature)
    {
        // Act & Assert
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
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WeatherData("Ramallah", 20, humidity)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_AcceptsHumidityBoundary(double humidity)
    {
        // Act
        var weather = new WeatherData("Ramallah", 20, humidity);

        // Assert
        Assert.Equal(humidity, weather.Humidity);
    }
}
