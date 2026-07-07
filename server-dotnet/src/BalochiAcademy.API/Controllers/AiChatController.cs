using BalochiAcademy.API.Services;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

/// <summary>
/// Token-gated AI endpoints — chat (AI Tutor) and single-shot generate (Dictionary, Join Forces,
/// Learn Coding, Simulations, ...). Ollama → Gemini → Ollama(extended) fallback chain.
/// Every call spends one AI token: the active subscription's per-period quota first,
/// then the student's persistent bonus tokens (bought with coins) once that's exhausted.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiChatController(AiService ai, ISystemSettingsService settings, IUnitOfWork uow, ICurrentUserService cu) : ControllerBase
{
    /// <summary>
    /// POST /api/ai/chat
    /// Send a chat message (with optional system prompt) and get an AI reply.
    /// The engine field in the response tells you which backend answered ("ollama" or "gemini").
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AiChatRequest req, CancellationToken ct)
    {
        if (await settings.GetAsync("ai_tutor_enabled", "true", ct) == "false")
            return StatusCode(503, new { error = "AI Tutor is currently disabled by the administrator." });

        if (req.Messages is not { Count: > 0 })
            return BadRequest(new { error = "messages array must not be empty" });

        var invalid = req.Messages.FirstOrDefault(m => m.Role is not ("user" or "assistant"));
        if (invalid != null)
            return BadRequest(new { error = $"Invalid role '{invalid.Role}'. Allowed: user, assistant" });

        var gate = await OpenTokenGateAsync(ct);
        if (gate.Error != null) return gate.Error;

        try
        {
            var (reply, engine) = await ai.ChatAsync(req.System, req.Messages, ct);
            var (tokensRemaining, bonusTokens) = await SpendTokenAsync(gate, ct);
            return Ok(new AiChatResponse(reply, engine, tokensRemaining, bonusTokens));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/ai/generate
    /// Single-shot prompt → AI reply (no conversation history). Used by the Dictionary, Join
    /// Forces, Learn Coding, and Simulations tools — same token metering as /chat.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] AiGenerateRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Prompt))
            return BadRequest(new { error = "prompt must not be empty" });

        var gate = await OpenTokenGateAsync(ct);
        if (gate.Error != null) return gate.Error;

        try
        {
            var (reply, engine) = await ai.ChatAsync(req.System, [new AiChatMessage("user", req.Prompt)], ct);
            var (tokensRemaining, bonusTokens) = await SpendTokenAsync(gate, ct);
            return Ok(new AiChatResponse(reply, engine, tokensRemaining, bonusTokens));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }

    // ── Shared token gate ────────────────────────────────────────────────────────

    private readonly record struct TokenGate(User User, UserSubscription? Sub, int PeriodicRemaining, IActionResult? Error);

    private async Task<TokenGate> OpenTokenGateAsync(CancellationToken ct)
    {
        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return new TokenGate(null!, null, 0, NotFound());

        var sub = await uow.Repository<UserSubscription>().Query()
            .Where(s => s.UserId == cu.UserId && s.Status == "active")
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);

        var periodicRemaining = sub == null ? 0 : Math.Max(0, sub.AiTokenQuota - sub.AiTokensUsedThisPeriod);
        if (periodicRemaining + user.BonusAiTokens <= 0)
        {
            return new TokenGate(user, sub, periodicRemaining, StatusCode(402, new
            {
                error = "You're out of AI tokens. Renew your subscription or buy more tokens to keep chatting.",
                outOfTokens = true,
            }));
        }

        return new TokenGate(user, sub, periodicRemaining, null);
    }

    private async Task<(int TokensRemainingThisPeriod, int BonusAiTokens)> SpendTokenAsync(TokenGate gate, CancellationToken ct)
    {
        if (gate.Sub != null && gate.PeriodicRemaining > 0) gate.Sub.AiTokensUsedThisPeriod++;
        else gate.User.BonusAiTokens = Math.Max(0, gate.User.BonusAiTokens - 1);
        await uow.SaveChangesAsync(ct);

        var tokensRemainingThisPeriod = gate.Sub == null ? 0 : Math.Max(0, gate.Sub.AiTokenQuota - gate.Sub.AiTokensUsedThisPeriod);
        return (tokensRemainingThisPeriod, gate.User.BonusAiTokens);
    }
}

public record AiChatRequest(
    List<AiChatMessage> Messages,
    string?             System = null   // optional system prompt (built client-side with grade/subject context)
);

public record AiGenerateRequest(
    string  Prompt,
    string? System = null
);

public record AiChatResponse(string Reply, string Engine, int TokensRemainingThisPeriod, int BonusAiTokens);
