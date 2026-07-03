using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BalochiAcademy.Application.Common.Interfaces;

namespace BalochiAcademy.API.Services;

/// <summary>
/// Sends chat messages to Ollama (local LLM) with automatic fallback to Google Gemini,
/// and a second Ollama retry at 3× timeout if Gemini is quota-limited.
///
/// Chain: Ollama (normal timeout) → Gemini → Ollama (extended timeout, only on 429/quota)
/// AI config is read from SystemSettings DB first, appsettings.json as fallback.
/// </summary>
public class AiService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<AiService> log,
    ISystemSettingsService settingsService)
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <returns>(reply text, engine name: "ollama" or "gemini")</returns>
    public async Task<(string Reply, string Engine)> ChatAsync(
        string?                     system,
        IReadOnlyList<AiChatMessage> messages,
        CancellationToken           ct = default)
    {
        // Load all AI config from DB (5-min cache), fall back to appsettings.json
        var ollamaUrl     = await settingsService.GetAsync("ollama_url",         config["Ai:OllamaUrl"]         ?? "http://localhost:11434", ct) ?? "http://localhost:11434";
        var ollamaModel   = await settingsService.GetAsync("ollama_model",       config["Ai:OllamaModel"]        ?? "llama3", ct) ?? "llama3";
        var ollamaTimeout = int.TryParse(await settingsService.GetAsync("ollama_timeout_sec", config["Ai:OllamaTimeoutSec"] ?? "30", ct), out var v) ? v : 30;
        var geminiKey     = await settingsService.GetAsync("gemini_api_key",     config["Ai:GeminiApiKey"]       ?? "", ct) ?? "";
        var geminiModel   = await settingsService.GetAsync("gemini_model",       config["Ai:GeminiModel"]        ?? "gemini-2.5-flash-lite", ct) ?? "gemini-2.5-flash-lite";

        string ollamaErr = string.Empty;

        // ── 1. Ollama (normal timeout) ─────────────────────────────────────────
        try
        {
            var reply = await OllamaChatAsync(system, messages, ollamaUrl, ollamaModel, ollamaTimeout, ct);
            return (reply, "ollama");
        }
        catch (Exception ex)
        {
            ollamaErr = ex.Message;
            log.LogWarning("Ollama failed ({Err}) — trying Gemini", ex.Message);
        }

        // ── 2. Gemini ──────────────────────────────────────────────────────────
        string geminiErr = string.Empty;
        bool   quotaHit  = false;
        try
        {
            var reply = await GeminiChatAsync(system, messages, geminiKey, geminiModel, ct);
            return (reply, "gemini");
        }
        catch (Exception ex)
        {
            geminiErr = ex.Message;
            quotaHit  = ex.Message.Contains("429")
                     || ex.Message.Contains("quota",   StringComparison.OrdinalIgnoreCase)
                     || ex.Message.Contains("billing", StringComparison.OrdinalIgnoreCase)
                     || ex.Message.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase);
            log.LogWarning("Gemini failed ({Err})", ex.Message);
        }

        // ── 3. Ollama extended timeout (only when Gemini is quota-limited) ─────
        if (quotaHit)
        {
            log.LogInformation(
                "Gemini quota exceeded — retrying Ollama with {Sec}s extended timeout",
                ollamaTimeout * 3);
            try
            {
                var reply = await OllamaChatAsync(system, messages, ollamaUrl, ollamaModel, ollamaTimeout * 3, ct);
                return (reply, "ollama");
            }
            catch (Exception ex)
            {
                log.LogError("Ollama extended timeout also failed: {Err}", ex.Message);
            }
        }

        throw new InvalidOperationException(
            $"All AI engines failed. Ollama: {ollamaErr} | Gemini: {geminiErr}");
    }

    // ── Ollama /api/chat ───────────────────────────────────────────────────────

    private async Task<string> OllamaChatAsync(
        string?                     system,
        IReadOnlyList<AiChatMessage> messages,
        string                      baseUrl,
        string                      model,
        int                         timeoutSec,
        CancellationToken           ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSec));

        var msgs = new List<object>();
        if (!string.IsNullOrWhiteSpace(system))
            msgs.Add(new { role = "system", content = system });
        msgs.AddRange(messages.Select(m => new { role = m.Role, content = m.Content }));

        var body = new
        {
            model    = model,
            messages = msgs,
            stream   = false,
            options  = new { temperature = 0.3 },
        };

        var http = httpFactory.CreateClient("ollama");
        // Override base address if it differs from what the named client was configured with
        if (http.BaseAddress?.ToString().TrimEnd('/') != baseUrl.TrimEnd('/'))
            http.BaseAddress = new Uri(baseUrl);

        var resp = await http.PostAsJsonAsync("/api/chat", body, JsonOpts, cts.Token);
        resp.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await resp.Content.ReadAsStreamAsync(cts.Token), cancellationToken: cts.Token);

        return doc.RootElement
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString()
               ?? throw new InvalidOperationException("Ollama returned empty content");
    }

    // ── Google Gemini generateContent ──────────────────────────────────────────

    private async Task<string> GeminiChatAsync(
        string?                     system,
        IReadOnlyList<AiChatMessage> messages,
        string                      apiKey,
        string                      model,
        CancellationToken           ct)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API key is not configured");

        var contents = messages
            .Select(m => new
            {
                role  = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } },
            })
            .ToArray();

        object body = string.IsNullOrWhiteSpace(system)
            ? new { contents }
            : new
            {
                systemInstruction = new { parts = new[] { new { text = system } } },
                contents,
            };

        var url  = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
        var http = httpFactory.CreateClient();
        var resp = await http.PostAsJsonAsync(url, body, JsonOpts, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"{(int)resp.StatusCode}: {err[..Math.Min(300, err.Length)]}");
        }

        using var doc = await JsonDocument.ParseAsync(
            await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        return doc.RootElement
                  .GetProperty("candidates")[0]
                  .GetProperty("content")
                  .GetProperty("parts")[0]
                  .GetProperty("text")
                  .GetString()
               ?? throw new InvalidOperationException("Gemini returned empty content");
    }
}

public record AiChatMessage(string Role, string Content);
