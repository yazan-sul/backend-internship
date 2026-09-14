using Microsoft.Extensions.Options;
using WeatherMonitoringService.Bots;
using WeatherMonitoringService.Configuration;
using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Tests.Bots;

public sealed class WeatherBotTests
{
    [Fact]
    public void RainBot_Activates_WhenHumidityIsAboveThreshold()
    {
        // Arrange
        var bot = new RainBot(
            Options.Create(
                new RainBotOptions
                {
                    Enabled = true,
                    HumidityThreshold = 70,
                    Message = "Rain message",
                }
            )
        );

        // Act
        var activation = bot.Notify(Weather(humidity: 71));

        // Assert
        Assert.NotNull(activation);
        Assert.Equal("RainBot", activation.Bot);
        Assert.Equal("Rain message", activation.Message);
    }

    [Theory]
    [InlineData(70)]
    [InlineData(69)]
    public void RainBot_DoesNotActivate_WhenHumidityIsNotAboveThreshold(double humidity)
    {
        // Arrange
        var bot = new RainBot(
            Options.Create(
                new RainBotOptions
                {
                    Enabled = true,
                    HumidityThreshold = 70,
                    Message = "Rain message",
                }
            )
        );

        // Act
        var activation = bot.Notify(Weather(humidity: humidity));

        // Assert
        Assert.Null(activation);
    }

    [Fact]
    public void SunBot_Activates_WhenTemperatureIsAboveThreshold()
    {
        // Arrange
        var bot = new SunBot(
            Options.Create(
                new SunBotOptions
                {
                    Enabled = true,
                    TemperatureThreshold = 30,
                    Message = "Sun message",
                }
            )
        );

        // Act
        var activation = bot.Notify(Weather(temperature: 31));

        // Assert
        Assert.NotNull(activation);
        Assert.Equal("SunBot", activation.Bot);
        Assert.Equal("Sun message", activation.Message);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(29)]
    public void SunBot_DoesNotActivate_WhenTemperatureIsNotAboveThreshold(double temperature)
    {
        // Arrange
        var bot = new SunBot(
            Options.Create(
                new SunBotOptions
                {
                    Enabled = true,
                    TemperatureThreshold = 30,
                    Message = "Sun message",
                }
            )
        );

        // Act
        var activation = bot.Notify(Weather(temperature: temperature));

        // Assert
        Assert.Null(activation);
    }

    [Fact]
    public void SnowBot_Activates_WhenTemperatureIsBelowThreshold()
    {
        // Arrange
        var bot = new SnowBot(
            Options.Create(
                new SnowBotOptions
                {
                    Enabled = true,
                    TemperatureThreshold = 0,
                    Message = "Snow message",
                }
            )
        );

        // Act
        var activation = bot.Notify(Weather(temperature: -1));

        // Assert
        Assert.NotNull(activation);
        Assert.Equal("SnowBot", activation.Bot);
        Assert.Equal("Snow message", activation.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void SnowBot_DoesNotActivate_WhenTemperatureIsNotBelowThreshold(double temperature)
    {
        // Arrange
        var bot = new SnowBot(
            Options.Create(
                new SnowBotOptions
                {
                    Enabled = true,
                    TemperatureThreshold = 0,
                    Message = "Snow message",
                }
            )
        );

        // Act
        var activation = bot.Notify(Weather(temperature: temperature));

        // Assert
        Assert.Null(activation);
    }

    [Fact]
    public void Bot_DoesNotActivate_WhenDisabled()
    {
        // Arrange
        var bot = new RainBot(
            Options.Create(
                new RainBotOptions
                {
                    Enabled = false,
                    HumidityThreshold = 70,
                    Message = "Rain message",
                }
            )
        );

        // Act
        var activation = bot.Notify(Weather(humidity: 100));

        // Assert
        Assert.Null(activation);
    }

    private static WeatherData Weather(double temperature = 20, double humidity = 50) =>
        new("Test City", temperature, humidity);
}
