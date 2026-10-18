using ServerStatisticsCollector;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<ServerStatisticsOptions>()
    .Bind(builder.Configuration.GetSection("ServerStatisticsConfig"))
    .Validate(
        options => options.SamplingIntervalSeconds > 0,
        "SamplingIntervalSeconds must be greater than 0."
    )
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ServerIdentifier),
        "ServerIdentifier is required."
    )
    .ValidateOnStart();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
