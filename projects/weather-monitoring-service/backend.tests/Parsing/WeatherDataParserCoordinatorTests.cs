using WeatherMonitoringService.Parsing;
using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Tests.Parsing;

public sealed class WeatherDataParserCoordinatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  \n\t")]
    public void Parse_RejectsEmptyInput(string? rawData)
    {
        var coordinator = CreateCoordinator();

        var exception = Assert.Throws<WeatherDataParsingException>(
            () => coordinator.Parse(rawData)
        );

        Assert.Equal("Weather data cannot be empty.", exception.Message);
    }

    [Fact]
    public void Parse_RejectsUnsupportedInput()
    {
        var coordinator = CreateCoordinator();

        var exception = Assert.Throws<WeatherDataParsingException>(
            () => coordinator.Parse("Location=Ramallah;Temperature=20;Humidity=50")
        );

        Assert.Contains("Unsupported", exception.Message);
    }

    [Fact]
    public void Parse_UsesFirstMatchingRegisteredStrategy()
    {
        var expected = new WeatherData("First", 20, 50);
        var coordinator = new WeatherDataParserCoordinator(
            new IWeatherDataParser[]
            {
                new StubParser(canHandle: true, expected),
                new StubParser(canHandle: true, new WeatherData("Second", 30, 60)),
            }
        );

        var result = coordinator.Parse("custom payload");

        Assert.Same(expected, result);
    }

    [Fact]
    public void Parse_PreservesFormatSpecificFailure()
    {
        var coordinator = CreateCoordinator();

        var exception = Assert.Throws<WeatherDataParsingException>(
            () => coordinator.Parse("{not valid JSON")
        );

        Assert.Contains("JSON", exception.Message);
        Assert.DoesNotContain("Unsupported", exception.Message);
    }

    private static WeatherDataParserCoordinator CreateCoordinator() =>
        new(new IWeatherDataParser[] { new JsonWeatherDataParser(), new XmlWeatherDataParser() });

    private sealed class StubParser(bool canHandle, WeatherData result) : IWeatherDataParser
    {
        public bool CanHandle(string rawData) => canHandle;

        public WeatherData Parse(string rawData) => result;
    }
}
