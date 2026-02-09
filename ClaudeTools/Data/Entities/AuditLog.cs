namespace ClaudeTools.Data.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Operation { get; set; } = "";
    public string? InputData { get; set; }
    public string? OutputData { get; set; }
    public string? PromptSent { get; set; }
    public string? RawResponse { get; set; }
    public int TokensIn { get; set; }
    public int TokensOut { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
