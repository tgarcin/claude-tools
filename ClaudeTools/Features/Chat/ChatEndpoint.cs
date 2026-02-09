namespace ClaudeTools.Features.Chat;

public static class ChatEndpoint
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/chat");

        group.MapPost("/", async (ChatRequest request, ChatService service) =>
        {
            try
            {
                var response = await service.AskAsync(request.Message);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return Results.Problem(ex.Message, statusCode: 502);
            }
        });
    }
}
