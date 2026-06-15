using BalochiAcademy.Application.Common;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Questions;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController(IApplicationDbContext db, ICurrentUserService cu) : ControllerBase
{
    private static QuestionDto Map(Question q) => new(
        q.Id, q.GradeCode, q.SubjectId, q.UnitId, q.TopicId,
        q.Kind, q.Stem, q.OptionsJson, q.CorrectIndex, q.QuestionType,
        q.Marks, q.ModelAnswer, q.Difficulty, q.CognitiveLevel,
        q.SloCode, q.IsEntranceExam, q.IsAiGenerated, q.Feedback);

    /// <summary>GET /api/questions?gradeCode=&amp;subjectId=&amp;kind=&amp;page=&amp;pageSize=</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<QuestionDto>>> GetQuestions(
        [FromQuery] QuestionFilterQuery q, CancellationToken ct)
    {
        var query = db.Questions.AsQueryable();
        if (!string.IsNullOrEmpty(q.GradeCode))    query = query.Where(x => x.GradeCode == q.GradeCode);
        if (q.SubjectId.HasValue)                  query = query.Where(x => x.SubjectId == q.SubjectId);
        if (q.UnitId.HasValue)                     query = query.Where(x => x.UnitId == q.UnitId);
        if (!string.IsNullOrEmpty(q.Kind))         query = query.Where(x => x.Kind == q.Kind);
        if (!string.IsNullOrEmpty(q.Difficulty))   query = query.Where(x => x.Difficulty == q.Difficulty);
        if (q.IsEntranceExam.HasValue)             query = query.Where(x => x.IsEntranceExam == q.IsEntranceExam);
        if (!string.IsNullOrEmpty(q.Search))       query = query.Where(x => x.Stem.Contains(q.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        return Ok(new PagedResult<QuestionDto>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            Page = q.Page,
            PageSize = q.PageSize,
        });
    }

    /// <summary>GET /api/questions/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionDto>> GetById(int id, CancellationToken ct)
    {
        var q = await db.Questions.FindAsync(id, ct);
        if (q == null) return NotFound();
        return Ok(Map(q));
    }

    /// <summary>POST /api/questions — admin only</summary>
    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<QuestionDto>> Create([FromBody] CreateQuestionRequest req, CancellationToken ct)
    {
        var question = new Question
        {
            Kind = req.Kind, Stem = req.Stem,
            GradeCode = req.GradeCode, SubjectId = req.SubjectId,
            UnitId = req.UnitId, TopicId = req.TopicId,
            OptionsJson = req.OptionsJson, CorrectIndex = req.CorrectIndex,
            QuestionType = req.QuestionType, Marks = req.Marks,
            ModelAnswer = req.ModelAnswer, Difficulty = req.Difficulty,
            CognitiveLevel = req.CognitiveLevel, Feedback = req.Feedback,
            SloCode = req.SloCode, IsEntranceExam = req.IsEntranceExam,
            CreatedById = cu.UserId,
        };
        db.Questions.Add(question);
        await db.SaveChangesAsync(ct);
        return Created($"/api/questions/{question.Id}", Map(question));
    }

    /// <summary>PUT /api/questions/{id} — admin only</summary>
    [HttpPut("{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<QuestionDto>> Update(int id, [FromBody] UpdateQuestionRequest req, CancellationToken ct)
    {
        var q = await db.Questions.FindAsync(id, ct);
        if (q == null) return NotFound();
        if (req.Stem          != null) q.Stem          = req.Stem;
        if (req.OptionsJson   != null) q.OptionsJson   = req.OptionsJson;
        if (req.CorrectIndex  != null) q.CorrectIndex  = req.CorrectIndex;
        if (req.ModelAnswer   != null) q.ModelAnswer   = req.ModelAnswer;
        if (req.Difficulty    != null) q.Difficulty    = req.Difficulty;
        if (req.CognitiveLevel != null) q.CognitiveLevel = req.CognitiveLevel;
        if (req.Feedback      != null) q.Feedback      = req.Feedback;
        if (req.Marks         != null) q.Marks         = req.Marks;
        await db.SaveChangesAsync(ct);
        return Ok(Map(q));
    }

    /// <summary>DELETE /api/questions/{id} — admin only</summary>
    [HttpDelete("{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var q = await db.Questions.FindAsync(id, ct);
        if (q == null) return NotFound();
        db.Questions.Remove(q);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>GET /api/questions/random?gradeCode=&amp;subjectId=&amp;count=30 — for self-tests</summary>
    [HttpGet("random")]
    public async Task<ActionResult<List<QuestionDto>>> Random(
        [FromQuery] string? gradeCode, [FromQuery] int? subjectId,
        [FromQuery] int? unitId, [FromQuery] string kind = "objective",
        [FromQuery] int count = 30, CancellationToken ct = default)
    {
        var query = db.Questions.Where(q => q.Kind == kind);
        if (!string.IsNullOrEmpty(gradeCode)) query = query.Where(q => q.GradeCode == gradeCode);
        if (subjectId.HasValue)               query = query.Where(q => q.SubjectId == subjectId);
        if (unitId.HasValue)                  query = query.Where(q => q.UnitId == unitId);

        var items = await query.OrderBy(_ => EF.Functions.Random()).Take(count).ToListAsync(ct);
        return Ok(items.Select(Map));
    }
}
