namespace ClaudeTools.Data.Entities;

public class SystemMetric
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double CpuPercent { get; set; }
    public double MemUsedMb { get; set; }
    public double MemTotalMb { get; set; }
    public double DiskUsedPercent { get; set; }
    public double NetRxBytesPerSec { get; set; }
    public double NetTxBytesPerSec { get; set; }
}
