using WeatherMonitoringService.Monitoring;
using WeatherMonitoringService.Parsing;

namespace WeatherMonitoringService.Api;

public static class WeatherUpdateEndpoint
{
    public static IEndpointRouteBuilder MapWeatherUpdateEndpoint(
        this IEndpointRouteBuilder endpoints
    )
    {
        endpoints.MapPost("/api/weather-updates", Handle);
        return endpoints;
    }

    private static IResult Handle(
        WeatherUpdateRequest? request,
        WeatherDataParserCoordinator parser,
        WeatherMonitor monitor
    )
    {
        try
        {
            var weather = parser.Parse(request?.RawData);
            var activations = monitor.Publish(weather);

            return Results.Ok(
                new WeatherUpdateResponse(
                    new WeatherResponse(
                        weather.Location,
                        weather.Temperature,
                        weather.Humidity
                    ),
                    activations
                        .Select(activation =>
                            new BotActivationResponse(activation.Bot, activation.Message)
                        )
                        .ToArray()
                )
            );
        }
        catch (WeatherDataParsingException exception)
        {
            return Results.BadRequest(new ApiErrorResponse(exception.Message));
        }
    }
}
