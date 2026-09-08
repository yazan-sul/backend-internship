using System.Globalization;
using WeatherMonitoringService.Parsing;

namespace WeatherMonitoringService.Tests.Parsing;

public sealed class XmlWeatherDataParserTests
{
    private readonly XmlWeatherDataParser _parser = new();

    [Fact]
    public void CanHandle_RecognizesXmlAfterWhitespace()
    {
        Assert.True(_parser.CanHandle(" \n<WeatherData />"));
        Assert.False(_parser.CanHandle("{\"Location\":\"Ramallah\"}"));
    }

    [Fact]
    public void Parse_ReturnsWeatherData_ForValidXml()
    {
        const string rawData =
            """
            <WeatherData>
              <Location> Ramallah </Location>
              <Temperature>24.5</Temperature>
              <Humidity>61</Humidity>
              <Ignored>true</Ignored>
            </WeatherData>
            """;

        var weather = _parser.Parse(rawData);

        Assert.Equal("Ramallah", weather.Location);
        Assert.Equal(24.5, weather.Temperature);
        Assert.Equal(61, weather.Humidity);
    }

    [Fact]
    public void Parse_UsesInvariantCultureForNumbers()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var weather = _parser.Parse(
                "<WeatherData><Location>Paris</Location><Temperature>24.5</Temperature><Humidity>61.5</Humidity></WeatherData>"
            );

            Assert.Equal(24.5, weather.Temperature);
            Assert.Equal(61.5, weather.Humidity);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("<WeatherData>")]
    [InlineData("<Other><Location>A</Location><Temperature>1</Temperature><Humidity>2</Humidity></Other>")]
    [InlineData("<WeatherData><Location>A</Location><Temperature>hot</Temperature><Humidity>2</Humidity></WeatherData>")]
    public void Parse_RejectsMalformedXml(string rawData)
    {
        var exception = Assert.Throws<WeatherDataParsingException>(() => _parser.Parse(rawData));

        Assert.Contains("XML", exception.Message);
    }

    [Theory]
    [InlineData("<WeatherData><Temperature>20</Temperature><Humidity>50</Humidity></WeatherData>")]
    [InlineData("<WeatherData><Location>A</Location><Humidity>50</Humidity></WeatherData>")]
    [InlineData("<WeatherData><Location>A</Location><Temperature>20</Temperature></WeatherData>")]
    [InlineData("<WeatherData><Location>A</Location><Location>B</Location><Temperature>20</Temperature><Humidity>50</Humidity></WeatherData>")]
    public void Parse_RejectsMissingOrDuplicateRequiredElements(string rawData)
    {
        var exception = Assert.Throws<WeatherDataParsingException>(() => _parser.Parse(rawData));

        Assert.Contains("exactly one", exception.Message);
    }

    [Theory]
    [InlineData("<WeatherData><Location> </Location><Temperature>20</Temperature><Humidity>50</Humidity></WeatherData>")]
    [InlineData("<WeatherData><Location>A</Location><Temperature>20</Temperature><Humidity>-1</Humidity></WeatherData>")]
    [InlineData("<WeatherData><Location>A</Location><Temperature>NaN</Temperature><Humidity>50</Humidity></WeatherData>")]
    public void Parse_RejectsInvalidWeatherValues(string rawData)
    {
        var exception = Assert.Throws<WeatherDataParsingException>(() => _parser.Parse(rawData));

        Assert.Contains("XML", exception.Message);
    }

    [Fact]
    public void Parse_RejectsDocumentsContainingDtds()
    {
        const string rawData =
            """
            <!DOCTYPE WeatherData [<!ENTITY location "Ramallah">]>
            <WeatherData>
              <Location>&location;</Location>
              <Temperature>20</Temperature>
              <Humidity>50</Humidity>
            </WeatherData>
            """;

        var exception = Assert.Throws<WeatherDataParsingException>(() => _parser.Parse(rawData));

        Assert.Contains("unsafe", exception.Message);
    }
}
