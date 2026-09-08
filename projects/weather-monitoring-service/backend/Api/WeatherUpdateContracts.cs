namespace WeatherMonitoringService.Api;

public sealed record WeatherUpdateRequest(string? RawData);

public sealed record WeatherUpdateResponse(
    WeatherResponse Weather,
    IReadOnlyList<BotActivationResponse> Activations
);

public sealed record WeatherResponse(string Location, double Temperature, double Humidity);

public sealed record BotActivationResponse(string Bot, string Message);

public sealed record ApiErrorResponse(string Error);
