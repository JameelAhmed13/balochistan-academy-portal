using System.Text.Json;
using BalochiAcademy.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalochiAcademy.API.Controllers;

/// <summary>
/// Server-side proxies for Google Gemini and Cloud TTS.
/// Keeps API keys off the browser — keys are read from ISystemSettingsService (DB)
/// with appsettings.json as fallback. Body is forwarded verbatim; status code is mirrored.
/// </summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiProxyController(
    IHttpClientFactory httpFactory,
    ISystemSettingsService settings,
    IConfiguration config) : ControllerBase
{
    /// <summary>
    /// POST /api/ai/gemini
    /// Forwards the request body to Google Gemini generateContent.
    /// Model is resolved from DB settings (gemini_model key) — the client does not pick it.
    /// </summary>
    [HttpPost("gemini")]
    public async Task<IActionResult> GeminiProxy([FromBody] JsonElement body, CancellationToken ct)
    {
        if (await settings.GetAsync("ai_tutor_enabled", "true", ct) == "false")
            return StatusCode(503, new { error = "AI features are currently disabled." });

        var key   = await settings.GetAsync("gemini_api_key", config["Ai:GeminiApiKey"] ?? "", ct) ?? "";
        var model = await settings.GetAsync("gemini_model",   config["Ai:GeminiModel"]   ?? "gemini-2.5-flash-lite", ct) ?? "gemini-2.5-flash-lite";

        if (string.IsNullOrWhiteSpace(key))
            return StatusCode(503, new { error = "Gemini API key is not configured." });

        var url  = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={key}";
        var http = httpFactory.CreateClient();
        var resp = await http.PostAsJsonAsync(url, body, ct);
        var content = await resp.Content.ReadAsStringAsync(ct);
        Response.StatusCode = (int)resp.StatusCode;
        return Content(content, "application/json");
    }

    /// <summary>
    /// POST /api/ai/tts
    /// Forwards the request body to Google Cloud Text-to-Speech synthesize.
    /// </summary>
    [HttpPost("tts")]
    public async Task<IActionResult> TtsProxy([FromBody] JsonElement body, CancellationToken ct)
    {
        if (await settings.GetAsync("video_lessons_enabled", "true", ct) == "false")
            return StatusCode(503, new { error = "Video Lessons are currently disabled." });

        var key = await settings.GetAsync("tts_api_key", config["Ai:TtsApiKey"] ?? "", ct) ?? "";

        if (string.IsNullOrWhiteSpace(key))
            return StatusCode(503, new { error = "TTS API key is not configured." });

        var url  = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={key}";
        var http = httpFactory.CreateClient();
        var resp = await http.PostAsJsonAsync(url, body, ct);
        var content = await resp.Content.ReadAsStringAsync(ct);
        Response.StatusCode = (int)resp.StatusCode;
        return Content(content, "application/json");
    }
}
