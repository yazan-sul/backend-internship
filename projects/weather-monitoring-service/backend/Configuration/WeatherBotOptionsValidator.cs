using FluentValidation;
using Microsoft.Extensions.Options;

namespace WeatherMonitoringService.Configuration;

public abstract class WeatherBotOptionsValidator<TOptions> : AbstractValidator<TOptions>, IValidateOptions<TOptions>
    where TOptions : WeatherBotOptions
{
    protected WeatherBotOptionsValidator(string sectionName)
    {
        RuleFor(options => options.Enabled).NotNull().WithMessage($"{sectionName}:enabled is required.");
        RuleFor(options => options.Message).Must(message => !string.IsNullOrWhiteSpace(message))
            .WithMessage($"{sectionName}:message is required and cannot be blank.");
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        var result = Validate(options);
        return result.IsValid
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(result.Errors.Select(error => error.ErrorMessage));
    }
}

public sealed class RainBotOptionsValidator : WeatherBotOptionsValidator<RainBotOptions>
{
    public RainBotOptionsValidator() : base(RainBotOptions.SectionName)
    {
        RuleFor(options => options.HumidityThreshold).NotNull().WithMessage("RainBot:humidityThreshold is required.")
            .Must(value => value is not null && double.IsFinite(value.Value))
            .WithMessage("RainBot:humidityThreshold must be finite.");
    }
}

public sealed class SunBotOptionsValidator : WeatherBotOptionsValidator<SunBotOptions>
{
    public SunBotOptionsValidator() : base(SunBotOptions.SectionName)
    {
        RuleFor(options => options.TemperatureThreshold).NotNull().WithMessage("SunBot:temperatureThreshold is required.")
            .Must(value => value is not null && double.IsFinite(value.Value))
            .WithMessage("SunBot:temperatureThreshold must be finite.");
    }
}

public sealed class SnowBotOptionsValidator : WeatherBotOptionsValidator<SnowBotOptions>
{
    public SnowBotOptionsValidator() : base(SnowBotOptions.SectionName)
    {
        RuleFor(options => options.TemperatureThreshold).NotNull().WithMessage("SnowBot:temperatureThreshold is required.")
            .Must(value => value is not null && double.IsFinite(value.Value))
            .WithMessage("SnowBot:temperatureThreshold must be finite.");
    }
}
