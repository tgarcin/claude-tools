using Microsoft.EntityFrameworkCore;
using ClaudeTools.Data.Entities;

namespace ClaudeTools.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemMetric> SystemMetrics => Set<SystemMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => a.Timestamp);
            e.HasIndex(a => a.Operation);
        });

        modelBuilder.Entity<SystemMetric>(e =>
        {
            e.HasIndex(m => m.Timestamp);
        });
    }
}
