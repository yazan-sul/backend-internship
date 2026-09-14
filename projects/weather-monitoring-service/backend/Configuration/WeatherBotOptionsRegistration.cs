using Microsoft.Extensions.Options;

namespace WeatherMonitoringService.Configuration;

public static class WeatherBotOptionsRegistration
{
    public static IServiceCollection AddWeatherBotOptions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<RainBotOptions>()
            .Bind(configuration.GetRequiredSection(RainBotOptions.SectionName))
            .ValidateOnStart();
        services
            .AddOptions<SunBotOptions>()
            .Bind(configuration.GetRequiredSection(SunBotOptions.SectionName))
            .ValidateOnStart();
        services
            .AddOptions<SnowBotOptions>()
            .Bind(configuration.GetRequiredSection(SnowBotOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RainBotOptions>, RainBotOptionsValidator>();
        services.AddSingleton<IValidateOptions<SunBotOptions>, SunBotOptionsValidator>();
        services.AddSingleton<IValidateOptions<SnowBotOptions>, SnowBotOptionsValidator>();

        return services;
    }
}
