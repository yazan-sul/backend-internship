using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherMonitoringService.Weather;

namespace WeatherMonitoringService.Parsing;

public sealed class JsonWeatherDataParser : IWeatherDataParser
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    };

    public bool CanHandle(string rawData)
    {
        if (string.IsNullOrWhiteSpace(rawData))
        {
            return false;
        }

        var firstCharacter = rawData.AsSpan().TrimStart()[0];
        return firstCharacter is '{' or '[';
    }

    public WeatherData Parse(string rawData)
    {
        try
        {
            var input = JsonSerializer.Deserialize<JsonWeatherData>(rawData, SerializerOptions);
            if (
                input is null
                || input.Location is null
                || input.Temperature is null
                || input.Humidity is null
            )
            {
                throw new WeatherDataParsingException(
                    "JSON weather data must include Location, Temperature, and Humidity."
                );
            }

            return new WeatherData(
                input.Location,
                input.Temperature.Value,
                input.Humidity.Value
            );
        }
        catch (WeatherDataParsingException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new WeatherDataParsingException(
                "JSON weather data is malformed.",
                exception
            );
        }
        catch (ArgumentException exception)
        {
            throw new WeatherDataParsingException(
                $"JSON weather data is invalid: {exception.Message}",
                exception
            );
        }
    }

    private sealed class JsonWeatherData
    {
        [JsonPropertyName("Location")]
        public string? Location { get; init; }

        [JsonPropertyName("Temperature")]
        public double? Temperature { get; init; }

        [JsonPropertyName("Humidity")]
        public double? Humidity { get; init; }
    }
}
