using Microsoft.EntityFrameworkCore;
using ClaudeTools.Data;

namespace ClaudeTools.Features.Audit;

public static class AuditEndpoint
{
    public static void MapAuditEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/audit");

        group.MapGet("/", async (int? limit, AppDbContext db) =>
        {
            var logs = await db.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(limit ?? 20)
                .Select(l => new
                {
                    l.Id,
                    l.Timestamp,
                    l.Operation,
                    l.InputData,
                    l.OutputData,
                    l.TokensIn,
                    l.TokensOut,
                    l.Success,
                    l.ErrorMessage
                })
                .ToListAsync();

            return Results.Ok(logs);
        });

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var log = await db.AuditLogs.FindAsync(id);
            return log is null ? Results.NotFound() : Results.Ok(log);
        });
    }
}
