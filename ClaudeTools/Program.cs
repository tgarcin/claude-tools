using Microsoft.EntityFrameworkCore;
using ClaudeTools.Data;
using ClaudeTools.Features.Chat;
using ClaudeTools.Features.Audit;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=claudetools.db"));

// Claude API service
builder.Services.AddHttpClient<ChatService>();
builder.Services.AddScoped<ChatService>();

// CORS for Vue dev server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JSON camelCase
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors();

// Endpoints
app.MapChatEndpoints();
app.MapAuditEndpoints();

app.Run();
