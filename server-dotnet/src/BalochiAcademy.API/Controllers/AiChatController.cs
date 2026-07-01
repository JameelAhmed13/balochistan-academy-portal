using BalochiAcademy.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalochiAcademy.API.Controllers;

/// <summary>
/// eStudy AI chat — Ollama → Gemini → Ollama(extended) fallback chain.
/// Used by the mobile app's eStudy AI screen and any other AI chat surfaces.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiChatController(AiService ai) : ControllerBase
{
    /// <summary>
    /// POST /api/ai/chat
    /// Send a chat message (with optional system prompt) and get an AI reply.
    /// The engine field in the response tells you which backend answered ("ollama" or "gemini").
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] AiChatRequest req,
        CancellationToken ct)
    {
        if (req.Messages is not { Count: > 0 })
            return BadRequest(new { error = "messages array must not be empty" });

        // Validate roles
        var invalid = req.Messages.FirstOrDefault(m =>
            m.Role is not ("user" or "assistant"));
        if (invalid != null)
            return BadRequest(new { error = $"Invalid role '{invalid.Role}'. Allowed: user, assistant" });

        try
        {
            var (reply, engine) = await ai.ChatAsync(req.System, req.Messages, ct);
            return Ok(new AiChatResponse(reply, engine));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }
}

public record AiChatRequest(
    List<AiChatMessage> Messages,
    string?             System = null   // optional system prompt (built client-side with grade/subject context)
);

public record AiChatResponse(string Reply, string Engine);
