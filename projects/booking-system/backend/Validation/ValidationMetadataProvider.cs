using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AirportTicketBookingSystem.Models;

namespace AirportTicketBookingSystem.Validation;

public sealed record ValidationFieldMetadata(
    string Field,
    string DisplayName,
    string Type,
    bool Required,
    object? Min,
    object? Max,
    int? MinLength,
    int? MaxLength,
    string[] Options,
    string[] CustomRules);

public sealed class ValidationMetadataProvider
{
    public ValidationFieldMetadata[] Details<T>() => typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(property => property.CanRead)
        .Select(CreateMetadata)
        .ToArray();

    private static ValidationFieldMetadata CreateMetadata(PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var range = property.GetCustomAttribute<RangeAttribute>();
        var length = property.GetCustomAttribute<StringLengthAttribute>();
        var rules = property.GetCustomAttributes<ValidationRuleAttribute>()
            .Select(attribute => attribute.Message)
            .ToArray();

        return new ValidationFieldMetadata(
            property.Name,
            property.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? property.Name,
            type.Name,
            property.GetCustomAttribute<RequiredAttribute>() is not null,
            range?.Minimum,
            range?.Maximum,
            length?.MinimumLength,
            length?.MaximumLength,
            type.IsEnum ? Enum.GetNames(type) : [],
            rules);
    }
}
