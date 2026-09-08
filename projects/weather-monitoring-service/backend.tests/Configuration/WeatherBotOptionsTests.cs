using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WeatherMonitoringService.Configuration;

namespace WeatherMonitoringService.Tests.Configuration;

public sealed class WeatherBotOptionsTests
{
    [Fact]
    public void Registration_BindsValidConfiguration()
    {
        using var provider = CreateServices(ValidSettings()).BuildServiceProvider();

        var rainBot = provider.GetRequiredService<IOptions<RainBotOptions>>().Value;
        var sunBot = provider.GetRequiredService<IOptions<SunBotOptions>>().Value;
        var snowBot = provider.GetRequiredService<IOptions<SnowBotOptions>>().Value;

        Assert.True(rainBot.Enabled);
        Assert.Equal(70, rainBot.HumidityThreshold);
        Assert.Equal("Rain message", rainBot.Message);
        Assert.True(sunBot.Enabled);
        Assert.Equal(30, sunBot.TemperatureThreshold);
        Assert.False(snowBot.Enabled);
        Assert.Equal(0, snowBot.TemperatureThreshold);
    }

    [Fact]
    public void Registration_AcceptsDisabledBotWithCompleteConfiguration()
    {
        var settings = ValidSettings();
        settings["RainBot:enabled"] = "false";
        using var provider = CreateServices(settings).BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<RainBotOptions>>().Value;

        Assert.False(options.Enabled);
    }

    [Fact]
    public void Registration_RejectsMissingSection()
    {
        var settings = ValidSettings();
        RemoveSection(settings, SnowBotOptions.SectionName);

        var exception = Assert.Throws<InvalidOperationException>(() => CreateServices(settings));

        Assert.Contains(SnowBotOptions.SectionName, exception.Message);
    }

    [Fact]
    public void Registration_RejectsMissingRequiredSetting()
    {
        var settings = ValidSettings();
        settings.Remove("RainBot:humidityThreshold");
        using var provider = CreateServices(settings).BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<RainBotOptions>>().Value
        );

        Assert.Contains("RainBot:humidityThreshold is required.", exception.Failures);
    }

    [Fact]
    public void Registration_RejectsMalformedSetting()
    {
        var settings = ValidSettings();
        settings["SunBot:temperatureThreshold"] = "hot";
        using var provider = CreateServices(settings).BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(
            () => provider.GetRequiredService<IOptions<SunBotOptions>>().Value
        );
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public void Registration_RejectsNonFiniteThreshold(string threshold)
    {
        var settings = ValidSettings();
        settings["SnowBot:temperatureThreshold"] = threshold;
        using var provider = CreateServices(settings).BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<SnowBotOptions>>().Value
        );

        Assert.Contains("SnowBot:temperatureThreshold must be finite.", exception.Failures);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Registration_RejectsMissingOrBlankMessage(string? message)
    {
        var settings = ValidSettings();

        if (message is null)
        {
            settings.Remove("RainBot:message");
        }
        else
        {
            settings["RainBot:message"] = message;
        }

        using var provider = CreateServices(settings).BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<RainBotOptions>>().Value
        );

        Assert.Contains("RainBot:message is required and cannot be blank.", exception.Failures);
    }

    private static ServiceCollection CreateServices(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddWeatherBotOptions(configuration);
        return services;
    }

    private static Dictionary<string, string?> ValidSettings() =>
        new()
        {
            ["RainBot:enabled"] = "true",
            ["RainBot:humidityThreshold"] = "70",
            ["RainBot:message"] = "Rain message",
            ["SunBot:enabled"] = "true",
            ["SunBot:temperatureThreshold"] = "30",
            ["SunBot:message"] = "Sun message",
            ["SnowBot:enabled"] = "false",
            ["SnowBot:temperatureThreshold"] = "0",
            ["SnowBot:message"] = "Snow message",
        };

    private static void RemoveSection(Dictionary<string, string?> settings, string sectionName)
    {
        foreach (
            var key in settings.Keys.Where(key => key.StartsWith($"{sectionName}:")).ToArray()
        )
        {
            settings.Remove(key);
        }
    }
}
