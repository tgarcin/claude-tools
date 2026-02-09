using ClaudeTools.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaudeTools.Features.SystemMonitor;

public static class SystemMonitorEndpoint
{
    public static void MapSystemMonitorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/monitor");

        group.MapGet("/current", (MetricsCollectorService collector) =>
        {
            var latest = collector.Latest;
            return latest is null
                ? Results.NoContent()
                : Results.Ok(latest);
        });

        group.MapGet("/history", async (int? minutes, AppDbContext db) =>
        {
            var since = DateTime.UtcNow.AddMinutes(-(minutes ?? 60));

            var metrics = await db.SystemMetrics
                .Where(m => m.Timestamp >= since)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    m.Timestamp,
                    m.CpuPercent,
                    m.MemUsedMb,
                    m.MemTotalMb,
                    m.DiskUsedPercent,
                    m.NetRxBytesPerSec,
                    m.NetTxBytesPerSec,
                })
                .ToListAsync();

            return Results.Ok(metrics);
        });
    }
}
