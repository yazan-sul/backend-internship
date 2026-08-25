using System.ComponentModel.DataAnnotations;
using System.Reflection;
namespace AirportTicketBookingSystem.Validation;
public sealed class ValidationMetadataProvider
{
    public object[] Details<T>() => typeof(T).GetProperties().Select(p => new { field = p.Name, type = Nullable.GetUnderlyingType(p.PropertyType)?.Name ?? p.PropertyType.Name, displayName = p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name, required = p.GetCustomAttribute<RequiredAttribute>() is not null, min = p.GetCustomAttribute<RangeAttribute>()?.Minimum, max = p.GetCustomAttribute<RangeAttribute>()?.Maximum, maxLength = p.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength, options = p.PropertyType.IsEnum ? Enum.GetNames(p.PropertyType) : Array.Empty<string>() }).Cast<object>().ToArray();
}
