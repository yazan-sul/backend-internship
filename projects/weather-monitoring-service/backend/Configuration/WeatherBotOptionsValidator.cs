using Microsoft.Extensions.Options;

namespace WeatherMonitoringService.Configuration;

public sealed class WeatherBotOptionsValidator :
    IValidateOptions<RainBotOptions>,
    IValidateOptions<SunBotOptions>,
    IValidateOptions<SnowBotOptions>
{
    public ValidateOptionsResult Validate(string? name, RainBotOptions options) =>
        Validate(
            RainBotOptions.SectionName,
            options,
            options.HumidityThreshold,
            "humidityThreshold"
        );

    public ValidateOptionsResult Validate(string? name, SunBotOptions options) =>
        Validate(
            SunBotOptions.SectionName,
            options,
            options.TemperatureThreshold,
            "temperatureThreshold"
        );

    public ValidateOptionsResult Validate(string? name, SnowBotOptions options) =>
        Validate(
            SnowBotOptions.SectionName,
            options,
            options.TemperatureThreshold,
            "temperatureThreshold"
        );

    private static ValidateOptionsResult Validate(
        string sectionName,
        WeatherBotOptions options,
        double? threshold,
        string thresholdName
    )
    {
        var failures = new List<string>();

        if (options.Enabled is null)
        {
            failures.Add($"{sectionName}:enabled is required.");
        }

        if (threshold is null)
        {
            failures.Add($"{sectionName}:{thresholdName} is required.");
        }
        else if (!double.IsFinite(threshold.Value))
        {
            failures.Add($"{sectionName}:{thresholdName} must be finite.");
        }

        if (string.IsNullOrWhiteSpace(options.Message))
        {
            failures.Add($"{sectionName}:message is required and cannot be blank.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
