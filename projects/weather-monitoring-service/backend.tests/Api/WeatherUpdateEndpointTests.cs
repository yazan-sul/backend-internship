using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WeatherMonitoringService.Tests.Api;

public sealed class WeatherUpdateEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Endpoint = "/api/weather-updates";
    private readonly HttpClient _client;

    public WeatherUpdateEndpointTests(WebApplicationFactory<Program> application)
    {
        _client = application.CreateClient();
    }

    [Fact]
    public async Task Post_WithJsonWeather_ReturnsNormalizedWeatherAndNoActivations()
    {
        const string rawData =
            """
            {"Location":"  Ramallah  ","Temperature":20,"Humidity":50}
            """;

        using var response = await PostAsync(rawData);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var document = await ReadJsonAsync(response);
        var root = document.RootElement;
        AssertPropertyNames(root, "weather", "activations");

        var weather = root.GetProperty("weather");
        AssertPropertyNames(weather, "location", "temperature", "humidity");
        Assert.Equal("Ramallah", weather.GetProperty("location").GetString());
        Assert.Equal(20, weather.GetProperty("temperature").GetDouble());
        Assert.Equal(50, weather.GetProperty("humidity").GetDouble());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("activations").ValueKind);
        Assert.Empty(root.GetProperty("activations").EnumerateArray());
    }

    [Fact]
    public async Task Post_WithXmlWeather_ReturnsNormalizedWeatherTransportShape()
    {
        const string rawData =
            """
            <WeatherData>
              <Location>Bethlehem</Location>
              <Temperature>31.5</Temperature>
              <Humidity>45</Humidity>
            </WeatherData>
            """;

        using var response = await PostAsync(rawData);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await ReadJsonAsync(response);
        var root = document.RootElement;
        AssertPropertyNames(root, "weather", "activations");

        var weather = root.GetProperty("weather");
        AssertPropertyNames(weather, "location", "temperature", "humidity");
        Assert.Equal("Bethlehem", weather.GetProperty("location").GetString());
        Assert.Equal(31.5, weather.GetProperty("temperature").GetDouble());
        Assert.Equal(45, weather.GetProperty("humidity").GetDouble());

        var activation = Assert.Single(root.GetProperty("activations").EnumerateArray());
        AssertPropertyNames(activation, "bot", "message");
        Assert.Equal("SunBot", activation.GetProperty("bot").GetString());
        Assert.Equal(
            "Wow, it's a scorcher out there!",
            activation.GetProperty("message").GetString()
        );
    }

    [Fact]
    public async Task Post_WhenMultipleBotsActivate_ReturnsEveryActivationInRegistrationOrder()
    {
        const string rawData =
            """
            {"Location":"Jericho","Temperature":35,"Humidity":80}
            """;

        using var response = await PostAsync(rawData);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = await ReadJsonAsync(response);
        var activations = document.RootElement.GetProperty("activations").EnumerateArray().ToArray();
        Assert.Equal(2, activations.Length);

        AssertPropertyNames(activations[0], "bot", "message");
        Assert.Equal("RainBot", activations[0].GetProperty("bot").GetString());
        Assert.Equal(
            "It looks like it's about to pour down!",
            activations[0].GetProperty("message").GetString()
        );

        AssertPropertyNames(activations[1], "bot", "message");
        Assert.Equal("SunBot", activations[1].GetProperty("bot").GetString());
        Assert.Equal(
            "Wow, it's a scorcher out there!",
            activations[1].GetProperty("message").GetString()
        );
    }

    [Theory]
    [InlineData(null, "cannot be empty")]
    [InlineData("{not valid JSON", "JSON")]
    [InlineData("Location=Ramallah;Temperature=20;Humidity=50", "Unsupported")]
    [InlineData("", "cannot be empty")]
    [InlineData("  \n\t", "cannot be empty")]
    public async Task Post_WithInvalidWeather_ReturnsBadRequestErrorShape(
        string? rawData,
        string expectedMessageFragment
    )
    {
        using var response = await PostAsync(rawData);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var document = await ReadJsonAsync(response);
        var root = document.RootElement;
        AssertPropertyNames(root, "error");
        var error = root.GetProperty("error").GetString();
        Assert.False(string.IsNullOrWhiteSpace(error));
        Assert.Contains(expectedMessageFragment, error, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<HttpResponseMessage> PostAsync(string? rawData) =>
        await _client.PostAsJsonAsync(Endpoint, new { rawData });

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync());

    private static void AssertPropertyNames(JsonElement element, params string[] expectedNames)
    {
        Assert.Equal(JsonValueKind.Object, element.ValueKind);
        Assert.Equal(expectedNames, element.EnumerateObject().Select(property => property.Name));
    }
}
