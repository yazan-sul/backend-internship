using Microsoft.Extensions.Options;

namespace ServerStatisticsCollector;

public class Worker : BackgroundService
{
    private readonly ServerStatisticsOptions _options;
    private readonly ILogger<Worker> _logger;
    public Worker(
       IOptions<ServerStatisticsOptions> options,
       ILogger<Worker> logger)
    {
        _options = options.Value;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
           TimeSpan.FromSeconds(_options.SamplingIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation(
                "Collecting statistics for {ServerIdentifier} at {Timestamp}",
                _options.ServerIdentifier,
                DateTime.UtcNow);
        }
    }
}
