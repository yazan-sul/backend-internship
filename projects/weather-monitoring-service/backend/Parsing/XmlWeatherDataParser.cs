using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Parsing;

public sealed class XmlWeatherDataParser : IWeatherDataParser
{
    private const string RootElementName = "WeatherData";

    public bool CanHandle(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
        {
            return false;
        }

        return rawData.AsSpan().TrimStart()[0] == '<';
    }

    public WeatherData Parse(string rawData)
    {
        try
        {
            using var stringReader = new StringReader(rawData);
            using var xmlReader = XmlReader.Create(
                stringReader,
                new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                }
            );
            var document = XDocument.Load(xmlReader, LoadOptions.None);
            var root = document.Root;

            if (root is null || root.Name != RootElementName)
            {
                throw new WeatherDataParsingException(
                    $"XML weather data must have a {RootElementName} root element."
                );
            }

            var location = GetRequiredElementValue(root, "Location");
            var temperature = ParseNumber(
                GetRequiredElementValue(root, "Temperature"),
                "Temperature"
            );
            var humidity = ParseNumber(GetRequiredElementValue(root, "Humidity"), "Humidity");

            return new WeatherData(location, temperature, humidity);
        }
        catch (WeatherDataParsingException)
        {
            throw;
        }
        catch (XmlException exception)
        {
            throw new WeatherDataParsingException(
                "XML weather data is malformed or unsafe.",
                exception
            );
        }
        catch (ArgumentException exception)
        {
            throw new WeatherDataParsingException(
                $"XML weather data is invalid: {exception.Message}",
                exception
            );
        }
    }

    private static string GetRequiredElementValue(XElement root, string elementName)
    {
        var elements = root.Elements(elementName).Take(2).ToArray();
        if (elements.Length != 1)
        {
            throw new WeatherDataParsingException(
                $"XML weather data must include exactly one {elementName} element."
            );
        }

        return elements[0].Value;
    }

    private static double ParseNumber(string value, string elementName)
    {
        if (
            !double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var number
            )
        )
        {
            throw new WeatherDataParsingException(
                $"XML {elementName} must be a valid number."
            );
        }

        return number;
    }
}
