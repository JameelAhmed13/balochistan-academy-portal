using BalochiAcademy.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

/// <summary>Provides DB-sourced context for the front-end AI tutor (RAG grounding).</summary>
[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController(IApplicationDbContext db) : ControllerBase
{
    private const int MaxSample    = 20;
    private const int MaxObjectives = 12;

    /// <summary>
    /// GET /api/ai/context?gradeCode=&amp;subjectId=&amp;sample=6
    /// Returns the syllabus outline + representative sample questions
    /// that the browser can inject into an Ollama/Gemini prompt.
    /// </summary>
    [HttpGet("context")]
    public async Task<IActionResult> Context(
        [FromQuery] string? gradeCode,
        [FromQuery] int?    subjectId,
        [FromQuery] int     sample    = 6,
        CancellationToken   ct        = default)
    {
        sample = Math.Min(sample, MaxSample);

        // ── syllabus: units + objectives ─────────────────────────────────────
        var syllabusItems = new List<object>();
        if (!string.IsNullOrEmpty(gradeCode) && subjectId.HasValue)
        {
            var units = await db.Units
                .Where(u => u.GradeCode == gradeCode && u.SubjectId == subjectId)
                .OrderBy(u => u.SortOrder).ThenBy(u => u.Number)
                .Select(u => new { u.Id, u.Name, u.Description })
                .ToListAsync(ct);

            var unitIds = units.Select(u => u.Id).ToList();
            var objectives = await db.LearningObjectives
                .Where(o => o.UnitId != null && unitIds.Contains(o.UnitId!.Value))
                .GroupBy(o => o.UnitId)
                .ToDictionaryAsync(
                    g => g.Key!.Value,
                    g => g.Select(o => o.ObjectiveText).Take(MaxObjectives).ToList(),
                    ct);

            syllabusItems = units.Select(u => (object)new
            {
                unit        = u.Name,
                description = u.Description,
                objectives  = objectives.TryGetValue(u.Id, out var objs) ? objs : new List<string>(),
            }).ToList();
        }

        // ── sample questions ─────────────────────────────────────────────────
        var qQuery = db.Questions.Where(q => q.Kind == "objective");
        if (!string.IsNullOrEmpty(gradeCode)) qQuery = qQuery.Where(q => q.GradeCode == gradeCode);
        if (subjectId.HasValue)               qQuery = qQuery.Where(q => q.SubjectId == subjectId);

        var sampleQuestions = await qQuery
            .OrderBy(_ => EF.Functions.Random())
            .Take(sample)
            .Select(q => new
            {
                q.Stem,
                Options = q.OptionsJson,
                Correct = q.CorrectIndex,
                Topic   = (string?)null,   // TopicText removed; keep field for API compat
            })
            .ToListAsync(ct);

        // ── plain-text snippet (for browser → Ollama injection) ─────────────
        var lines = new List<string>();
        if (syllabusItems.Any())
        {
            lines.Add($"Syllabus for Grade {gradeCode} Subject #{subjectId}:");
            foreach (dynamic item in syllabusItems)
            {
                var desc = string.IsNullOrWhiteSpace((string?)item.description)
                    ? "" : $": {item.description}";
                lines.Add($"- {item.unit}{desc}");
                foreach (var obj in (List<string>)item.objectives)
                    lines.Add($"    • {obj}");
            }
        }
        if (sampleQuestions.Any())
        {
            lines.Add("");
            lines.Add("Representative real exam questions (style/difficulty reference):");
            for (var i = 0; i < sampleQuestions.Count; i++)
                lines.Add($"{i + 1}. {sampleQuestions[i].Stem}");
        }

        return Ok(new
        {
            gradeCode,
            subjectId,
            syllabus        = syllabusItems,
            sampleQuestions,
            contextText     = string.Join("\n", lines),
        });
    }
}
