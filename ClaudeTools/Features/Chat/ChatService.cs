using System.Text.Json;
using System.Text.Json.Serialization;
using ClaudeTools.Data;
using ClaudeTools.Data.Entities;

namespace ClaudeTools.Features.Chat;

public class ChatService
{
    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<ChatService> _logger;

    public ChatService(HttpClient http, AppDbContext db, IConfiguration config, ILogger<ChatService> logger)
    {
        _http = http;
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<ChatResponse> AskAsync(string userMessage)
    {
        var apiKey = _config["Claude:ApiKey"]
            ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
            ?? throw new InvalidOperationException("Claude API key not configured");

        var request = new
        {
            model = _config["Claude:Model"] ?? "claude-sonnet-4-5-20250929",
            max_tokens = 1024,
            messages = new[] { new { role = "user", content = userMessage } }
        };

        var prompt = JsonSerializer.Serialize(request);

        var audit = new AuditLog
        {
            Operation = "chat",
            InputData = userMessage,
            PromptSent = prompt
        };

        try
        {
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _http.PostAsJsonAsync("https://api.anthropic.com/v1/messages", request);
            var rawJson = await response.Content.ReadAsStringAsync();
            audit.RawResponse = rawJson;

            if (!response.IsSuccessStatusCode)
            {
                audit.Success = false;
                audit.ErrorMessage = $"HTTP {(int)response.StatusCode}: {rawJson}";
                throw new HttpRequestException(audit.ErrorMessage);
            }

            var result = JsonSerializer.Deserialize<ClaudeApiResponse>(rawJson);
            var text = result?.Content?.FirstOrDefault()?.Text ?? "";

            audit.Success = true;
            audit.OutputData = text;
            audit.TokensIn = result?.Usage?.InputTokens ?? 0;
            audit.TokensOut = result?.Usage?.OutputTokens ?? 0;

            return new ChatResponse(text, audit.TokensIn, audit.TokensOut);
        }
        catch (Exception ex) when (ex is not HttpRequestException)
        {
            audit.Success = false;
            audit.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            _db.AuditLogs.Add(audit);
            await _db.SaveChangesAsync();
        }
    }
}

public record ChatResponse(string Text, int TokensIn, int TokensOut);

public record ChatRequest(string Message);

// Claude API response models
file class ClaudeApiResponse
{
    [JsonPropertyName("content")]
    public List<ContentBlock>? Content { get; set; }

    [JsonPropertyName("usage")]
    public UsageInfo? Usage { get; set; }
}

file class ContentBlock
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

file class UsageInfo
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }
}
