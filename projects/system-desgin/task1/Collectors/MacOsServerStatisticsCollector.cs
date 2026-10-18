using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ServerStatisticsCollector;

public class MacOsServerStatisticsCollector : IServerStatisticsCollector
{
    public async Task<ServerStatistics> CollectAsync(CancellationToken cancellationToken)
    {
        var cpuUsage = await GetCpuUsageAsync(cancellationToken);
        var memory = await GetRamAsync(cancellationToken);

        return new ServerStatistics
        {
            CpuUsage = cpuUsage,
            AvailableMemory = memory.Available,
            MemoryUsage = memory.Used,
            Timestamp = DateTime.UtcNow
        };
    }

    private static async Task<double> GetCpuUsageAsync(
        CancellationToken cancellationToken = default)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "/usr/bin/top",
            Arguments = "-l 2 -s 1 -n 0",
            RedirectStandardOutput = true,
            Environment = { ["LC_ALL"] = "C" }
        }) ?? throw new InvalidOperationException("Could not start top.");

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
            throw new InvalidOperationException("top failed.");

        var matches = Regex.Matches(
            output, @"CPU usage:[^\r\n]*?([\d.]+)% idle");
        if (matches.Count < 2)
            throw new FormatException("Missing CPU samples in top output.");

        var idle = double.Parse(
            matches[^1].Groups[1].Value,
            CultureInfo.InvariantCulture);

        return 100 - idle;
    }
    private static async Task<(double Used, double Available)> GetRamAsync(
        CancellationToken cancellationToken = default)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "/usr/bin/vm_stat",
            RedirectStandardOutput = true
        }) ?? throw new InvalidOperationException("Could not start vm_stat.");
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"vm_stat exited with code {process.ExitCode}.");

        var pageSize = ParseMemoryValue(output, @"page size of (\d+) bytes");
        var free = ParseMemoryValue(output, @"^Pages free:\s*(\d+)\.");
        var inactive = ParseMemoryValue(output, @"^Pages inactive:\s*(\d+)\.");
        var availableMemory = (free + inactive) * pageSize / 1_000_000;

        using var memoryProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "/usr/sbin/sysctl",
            Arguments = "-n hw.memsize",
            RedirectStandardOutput = true
        }) ?? throw new InvalidOperationException("Could not start sysctl.");
        var totalMemoryOutput = await memoryProcess.StandardOutput.ReadToEndAsync(cancellationToken);
        await memoryProcess.WaitForExitAsync(cancellationToken);
        if (memoryProcess.ExitCode != 0)
            throw new InvalidOperationException($"sysctl exited with code {memoryProcess.ExitCode}.");

        var totalMemoryMb = double.Parse(totalMemoryOutput.Trim()) / 1_000_000;
        return (totalMemoryMb - availableMemory, availableMemory);
    }

    private static double ParseMemoryValue(string output, string pattern)
    {
        var match = Regex.Match(output, pattern, RegexOptions.Multiline);
        if (!match.Success)
            throw new FormatException($"Missing memory value in vm_stat output: {pattern}");

        return double.Parse(match.Groups[1].Value);
    }
}

