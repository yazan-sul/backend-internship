using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AirportTicketBookingSystem.Validation;

public sealed class ValidationMetadataProvider
{
    public object[] Details<T>() => typeof(T)
        .GetProperties()
        .Select(property => new
        {
            field = property.Name,
            type = Nullable.GetUnderlyingType(property.PropertyType)?.Name ?? property.PropertyType.Name,
            displayName = property.GetCustomAttribute<DisplayAttribute>()?.Name ?? property.Name,
            required = property.GetCustomAttribute<RequiredAttribute>() is not null,
            min = property.GetCustomAttribute<RangeAttribute>()?.Minimum,
            max = property.GetCustomAttribute<RangeAttribute>()?.Maximum,
            maxLength = property.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength,
            options = property.PropertyType.IsEnum
                ? Enum.GetNames(property.PropertyType)
                : Array.Empty<string>()
        })
        .Cast<object>()
        .ToArray();
}
