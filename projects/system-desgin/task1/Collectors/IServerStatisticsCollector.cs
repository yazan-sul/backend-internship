namespace ServerStatisticsCollector;

public interface IServerStatisticsCollector
{
    Task<ServerStatistics> CollectAsync(
        CancellationToken cancellationToken);
}
