namespace AirportTicketBookingSystem.Models;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValidationRuleAttribute(string message) : Attribute
{
    public string Message { get; } = message;
}
