using Microsoft.AspNetCore.Diagnostics;

namespace RestaurantReservation.API.Errors;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception is BadHttpRequestException badRequest
            ? badRequest.StatusCode
            : StatusCodes.Status500InternalServerError;

        if (status >= 500)
            logger.LogError(exception, "Request failed. Trace ID: {TraceId}", context.TraceIdentifier);

        var detail = status switch
        {
            400 => "Send a valid JSON object with customerId, restaurantId, tableId, partySize, "
                + "and reservationDate. Use whole numbers for IDs and party size, and a UTC date such as 2027-01-20T18:00:00Z.",
            413 => "The request is too large. Send a smaller request body.",
            415 => "Send the request body as JSON with Content-Type: application/json.",
            _ => "We could not complete your request. Please try again. If the problem continues, contact support with the trace ID."
        };

        await Results.Problem(
            statusCode: status,
            title: status >= 500 ? "Something went wrong." : "Invalid request.",
            detail: detail).ExecuteAsync(context);
        return true;
    }
}
