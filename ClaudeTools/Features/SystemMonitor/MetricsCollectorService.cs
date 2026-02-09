using ClaudeTools.Data;
using ClaudeTools.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaudeTools.Features.SystemMonitor;

public class MetricsCollectorService : IHostedService, IDisposable
{
    private readonly SystemMonitorService _monitor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricsCollectorService> _logger;
    private PeriodicTimer? _timer;
    private Task? _backgroundTask;
    private CancellationTokenSource? _cts;

    private volatile SystemSnapshot? _latest;
    private int _tickCount;
    private const int PersistEveryNTicks = 15; // every 30s (15 * 2s)

    public SystemSnapshot? Latest => _latest;

    public MetricsCollectorService(
        SystemMonitorService monitor,
        IServiceScopeFactory scopeFactory,
        ILogger<MetricsCollectorService> logger)
    {
        _monitor = monitor;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MetricsCollector starting (2s interval, persist every {Seconds}s)",
            PersistEveryNTicks * 2);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        _backgroundTask = RunAsync(_cts.Token);

        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken ct)
    {
        // Initial capture to prime CPU delta
        try { _latest = await _monitor.CaptureAsync(); }
        catch (Exception ex) { _logger.LogWarning(ex, "Initial capture failed"); }

        while (await _timer!.WaitForNextTickAsync(ct))
        {
            try
            {
                _latest = await _monitor.CaptureAsync();
                _tickCount++;

                if (_tickCount % PersistEveryNTicks == 0)
                {
                    await PersistAsync(_latest);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Metrics capture failed");
            }
        }
    }

    private async Task PersistAsync(SystemSnapshot snapshot)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.SystemMetrics.Add(new SystemMetric
            {
                Timestamp = snapshot.Timestamp,
                CpuPercent = snapshot.CpuPercent,
                MemUsedMb = snapshot.MemUsedMb,
                MemTotalMb = snapshot.MemTotalMb,
                DiskUsedPercent = snapshot.DiskUsedPercent,
                NetRxBytesPerSec = snapshot.NetRxBytesPerSec,
                NetTxBytesPerSec = snapshot.NetTxBytesPerSec,
            });

            await db.SaveChangesAsync();

            // Prune old records (keep last 24h)
            var cutoff = DateTime.UtcNow.AddHours(-24);
            await db.SystemMetrics.Where(m => m.Timestamp < cutoff).ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist metrics");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MetricsCollector stopping");
        _cts?.Cancel();
        return _backgroundTask ?? Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts?.Dispose();
        _timer?.Dispose();
    }
}
