using Microsoft.AspNetCore.Diagnostics;

namespace WeatherMonitoringService.Api;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var isBadRequest = exception is BadHttpRequestException;

        httpContext.Response.StatusCode = isBadRequest
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(
                isBadRequest ? "The request body is invalid." : "An unexpected error occurred."
            ),
            cancellationToken
        );

        return true;
    }
}
