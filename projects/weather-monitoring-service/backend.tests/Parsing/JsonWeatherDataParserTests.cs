using WeatherMonitoringService.Parsing;

namespace WeatherMonitoringService.Tests.Parsing;

public sealed class JsonWeatherDataParserTests
{
    private readonly JsonWeatherDataParser _parser = new();

    [Fact]
    public void CanHandle_RecognizesJsonAfterWhitespace()
    {
        // Act & Assert
        Assert.True(_parser.CanHandle("  \n {\"Location\":\"Ramallah\"}"));
        Assert.True(_parser.CanHandle("\t[1, 2]"));
        Assert.False(_parser.CanHandle("<WeatherData />"));
    }

    [Fact]
    public void Parse_ReturnsWeatherData_ForValidJson()
    {
        // Arrange
        const string rawData =
            """
            {
              "Location": " Ramallah ",
              "Temperature": 24.5,
              "Humidity": 61,
              "Ignored": true
            }
            """;

        // Act
        var weather = _parser.Parse(rawData);

        // Assert
        Assert.Equal("Ramallah", weather.Location);
        Assert.Equal(24.5, weather.Temperature);
        Assert.Equal(61, weather.Humidity);
    }

    [Theory]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("{\"Location\": 123, \"Temperature\": 20, \"Humidity\": 50}")]
    public void Parse_RejectsMalformedOrWronglyTypedJson(string rawData)
    {
        // Act & Assert
        var exception = Assert.Throws<WeatherDataParsingException>(() => _parser.Parse(rawData));

        Assert.Contains("JSON", exception.Message);
    }

    [Theory]
    [InlineData("{\"Temperature\":20,\"Humidity\":50}")]
    [InlineData("{\"Location\":\"Ramallah\",\"Humidity\":50}")]
    [InlineData("{\"Location\":\"Ramallah\",\"Temperature\":20}")]
    [InlineData("{\"location\":\"Ramallah\",\"Temperature\":20,\"Humidity\":50}")]
    public void Parse_RejectsMissingRequiredProperties(string rawData)
    {
        // Act & Assert
        var exception = Assert.Throws<WeatherDataParsingException>(() => _parser.Parse(rawData));

        Assert.Contains("must include", exception.Message);
    }

    [Theory]
    [InlineData("{\"Location\":\" \",\"Temperature\":20,\"Humidity\":50}")]
    [InlineData("{\"Location\":\"Ramallah\",\"Temperature\":20,\"Humidity\":101}")]
    [InlineData("{\"Location\":\"Ramallah\",\"Temperature\":1e400,\"Humidity\":50}")]
    public void Parse_RejectsInvalidWeatherValues(string rawData)
    {
        // Act & Assert
        var exception = Assert.Throws<WeatherDataParsingException>(() => _parser.Parse(rawData));

        Assert.Contains("JSON", exception.Message);
    }
}
