namespace ClaudeTools.Features.SystemMonitor;

public record SystemSnapshot(
    double CpuPercent,
    double MemUsedMb,
    double MemTotalMb,
    double DiskUsedPercent,
    double NetRxBytesPerSec,
    double NetTxBytesPerSec,
    DateTime Timestamp);

public class SystemMonitorService
{
    private long _prevCpuTotal;
    private long _prevCpuIdle;
    private long _prevNetRx;
    private long _prevNetTx;
    private DateTime _prevNetTime = DateTime.MinValue;

    public async Task<SystemSnapshot> CaptureAsync()
    {
        var cpu = await ReadCpuAsync();
        var mem = await ReadMemoryAsync();
        var disk = ReadDisk();
        var net = await ReadNetworkAsync();

        return new SystemSnapshot(
            CpuPercent: Math.Round(cpu, 1),
            MemUsedMb: Math.Round(mem.UsedMb, 0),
            MemTotalMb: Math.Round(mem.TotalMb, 0),
            DiskUsedPercent: Math.Round(disk, 1),
            NetRxBytesPerSec: Math.Round(net.RxPerSec, 0),
            NetTxBytesPerSec: Math.Round(net.TxPerSec, 0),
            Timestamp: DateTime.UtcNow);
    }

    private async Task<double> ReadCpuAsync()
    {
        var line = (await File.ReadAllLinesAsync("/proc/stat"))[0]; // "cpu  user nice system idle ..."
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // parts[0] = "cpu", parts[1..] = user, nice, system, idle, iowait, irq, softirq, steal
        long user = long.Parse(parts[1]);
        long nice = long.Parse(parts[2]);
        long system = long.Parse(parts[3]);
        long idle = long.Parse(parts[4]);
        long iowait = long.Parse(parts[5]);
        long irq = long.Parse(parts[6]);
        long softirq = long.Parse(parts[7]);
        long steal = parts.Length > 8 ? long.Parse(parts[8]) : 0;

        long totalIdle = idle + iowait;
        long total = user + nice + system + idle + iowait + irq + softirq + steal;

        long deltaTotal = total - _prevCpuTotal;
        long deltaIdle = totalIdle - _prevCpuIdle;

        _prevCpuTotal = total;
        _prevCpuIdle = totalIdle;

        if (deltaTotal == 0) return 0;
        return (1.0 - (double)deltaIdle / deltaTotal) * 100.0;
    }

    private static async Task<(double TotalMb, double UsedMb)> ReadMemoryAsync()
    {
        var lines = await File.ReadAllLinesAsync("/proc/meminfo");
        long totalKb = 0, availableKb = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("MemTotal:"))
                totalKb = ParseMemInfoKb(line);
            else if (line.StartsWith("MemAvailable:"))
                availableKb = ParseMemInfoKb(line);
        }

        double totalMb = totalKb / 1024.0;
        double usedMb = (totalKb - availableKb) / 1024.0;
        return (totalMb, usedMb);
    }

    private static long ParseMemInfoKb(string line)
    {
        // "MemTotal:       16384000 kB"
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return long.Parse(parts[1]);
    }

    private static double ReadDisk()
    {
        try
        {
            var drive = new DriveInfo("/");
            if (drive.TotalSize == 0) return 0;
            double usedPercent = (1.0 - (double)drive.AvailableFreeSpace / drive.TotalSize) * 100.0;
            return usedPercent;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<(double RxPerSec, double TxPerSec)> ReadNetworkAsync()
    {
        var lines = await File.ReadAllLinesAsync("/proc/net/dev");
        long totalRx = 0, totalTx = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!trimmed.Contains(':') || trimmed.StartsWith("Inter") || trimmed.StartsWith("face"))
                continue;

            // Skip loopback
            if (trimmed.StartsWith("lo:"))
                continue;

            var colonIdx = trimmed.IndexOf(':');
            var data = trimmed[(colonIdx + 1)..].Trim();
            var parts = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 9)
            {
                totalRx += long.Parse(parts[0]); // bytes received
                totalTx += long.Parse(parts[8]); // bytes transmitted
            }
        }

        var now = DateTime.UtcNow;
        double rxPerSec = 0, txPerSec = 0;

        if (_prevNetTime != DateTime.MinValue)
        {
            var elapsed = (now - _prevNetTime).TotalSeconds;
            if (elapsed > 0)
            {
                rxPerSec = (totalRx - _prevNetRx) / elapsed;
                txPerSec = (totalTx - _prevNetTx) / elapsed;
                if (rxPerSec < 0) rxPerSec = 0;
                if (txPerSec < 0) txPerSec = 0;
            }
        }

        _prevNetRx = totalRx;
        _prevNetTx = totalTx;
        _prevNetTime = now;

        return (rxPerSec, txPerSec);
    }
}
