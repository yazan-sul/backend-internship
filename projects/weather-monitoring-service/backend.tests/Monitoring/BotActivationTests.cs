using WeatherMonitoringService.Monitoring;

namespace WeatherMonitoringService.Tests.Monitoring;

public sealed class BotActivationTests
{
    [Fact]
    public void Constructor_CreatesActivation_WhenValuesAreValid()
    {
        // Act
        var activation = new BotActivation("RainBot", "Rain is likely.");

        // Assert
        Assert.Equal("RainBot", activation.Bot);
        Assert.Equal("Rain is likely.", activation.Message);
    }

    [Theory]
    [InlineData("", "message")]
    [InlineData(" ", "message")]
    [InlineData("RainBot", "")]
    [InlineData("RainBot", " ")]
    public void Constructor_RejectsBlankValues(string bot, string message)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new BotActivation(bot, message));
    }
}
