using AutoMapper;
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
public class QuestionsController(IUnitOfWork uow, ICurrentUserService cu, IMapper mapper, IApplicationDbContext db) : ControllerBase
{
    /// <summary>GET /api/questions?gradeCode=&amp;subjectId=&amp;kind=&amp;page=&amp;pageSize=</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<QuestionDto>>> GetQuestions(
        [FromQuery] QuestionFilterQuery q, CancellationToken ct)
    {
        var query = uow.Repository<Question>().Query();
        if (!string.IsNullOrEmpty(q.GradeCode))  query = query.Where(x => x.GradeCode == q.GradeCode);
        if (q.SubjectId.HasValue)                 query = query.Where(x => x.SubjectId == q.SubjectId);
        if (q.UnitId.HasValue)                    query = query.Where(x => x.UnitId == q.UnitId);
        if (!string.IsNullOrEmpty(q.Kind))        query = query.Where(x => x.Kind == q.Kind);
        if (!string.IsNullOrEmpty(q.Difficulty))  query = query.Where(x => x.Difficulty == q.Difficulty);
        if (q.IsEntranceExam.HasValue)            query = query.Where(x => x.IsEntranceExam == q.IsEntranceExam);
        if (!string.IsNullOrEmpty(q.Search))      query = query.Where(x => x.Stem.Contains(q.Search));
        if (!string.IsNullOrEmpty(q.SubjectName))
        {
            var sid = await db.Subjects.Where(s => s.Name == q.SubjectName)
                .Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
            if (sid.HasValue) query = query.Where(x => x.SubjectId == sid.Value);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        return Ok(new PagedResult<QuestionDto>
        {
            Items      = mapper.Map<List<QuestionDto>>(items),
            TotalCount = total,
            Page       = q.Page,
            PageSize   = q.PageSize,
        });
    }

    /// <summary>GET /api/questions/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuestionDto>> GetById(int id, CancellationToken ct)
    {
        var q = await uow.Repository<Question>().FindAsync([id], ct);
        if (q == null) return NotFound();
        return Ok(mapper.Map<QuestionDto>(q));
    }

    /// <summary>POST /api/questions — admin only</summary>
    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<QuestionDto>> Create([FromBody] CreateQuestionRequest req, CancellationToken ct)
    {
        var question = new Question
        {
            Kind           = req.Kind,        Stem         = req.Stem,
            GradeCode      = req.GradeCode,   SubjectId    = req.SubjectId,
            UnitId         = req.UnitId,      TopicId      = req.TopicId,
            OptionsJson    = req.OptionsJson,  CorrectIndex = req.CorrectIndex,
            QuestionType   = req.QuestionType, Marks        = req.Marks,
            ModelAnswer    = req.ModelAnswer,  Difficulty   = req.Difficulty,
            CognitiveLevel = req.CognitiveLevel, Feedback   = req.Feedback,
            SloCode        = req.SloCode,     IsEntranceExam = req.IsEntranceExam,
            IsAiGenerated  = req.IsAiGenerated, CreatedById   = cu.UserId,
            PastPaperId    = req.PastPaperId,
        };
        uow.Repository<Question>().Add(question);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/questions/{question.Id}", mapper.Map<QuestionDto>(question));
    }

    /// <summary>PUT /api/questions/{id} — admin only</summary>
    [HttpPut("{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<QuestionDto>> Update(int id, [FromBody] UpdateQuestionRequest req, CancellationToken ct)
    {
        var q = await uow.Repository<Question>().FindAsync([id], ct);
        if (q == null) return NotFound();
        if (req.Stem           != null) q.Stem          = req.Stem;
        if (req.OptionsJson    != null) q.OptionsJson    = req.OptionsJson;
        if (req.CorrectIndex   != null) q.CorrectIndex   = req.CorrectIndex;
        if (req.ModelAnswer    != null) q.ModelAnswer    = req.ModelAnswer;
        if (req.Difficulty     != null) q.Difficulty     = req.Difficulty;
        if (req.CognitiveLevel != null) q.CognitiveLevel = req.CognitiveLevel;
        if (req.Feedback       != null) q.Feedback       = req.Feedback;
        if (req.Marks          != null) q.Marks          = req.Marks;
        if (req.ClearPastPaper) q.PastPaperId = null;
        else if (req.PastPaperId != null) q.PastPaperId = req.PastPaperId;
        await uow.SaveChangesAsync(ct);
        return Ok(mapper.Map<QuestionDto>(q));
    }

    /// <summary>DELETE /api/questions/{id} — admin only</summary>
    [HttpDelete("{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var q = await uow.Repository<Question>().FindAsync([id], ct);
        if (q == null) return NotFound();
        uow.Repository<Question>().Remove(q);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>GET /api/questions/random?gradeCode=&amp;subjectId=&amp;count=30</summary>
    [HttpGet("random")]
    public async Task<ActionResult<List<QuestionDto>>> Random(
        [FromQuery] string? gradeCode, [FromQuery] int? subjectId,
        [FromQuery] string? subjectName = null,
        [FromQuery] int? unitId = null, [FromQuery] string kind = "objective",
        [FromQuery] int count = 30, CancellationToken ct = default)
    {
        var query = uow.Repository<Question>().Query().Where(q => q.Kind == kind);
        if (!string.IsNullOrEmpty(gradeCode)) query = query.Where(q => q.GradeCode == gradeCode);
        if (subjectId.HasValue)               query = query.Where(q => q.SubjectId == subjectId);
        if (!subjectId.HasValue && !string.IsNullOrEmpty(subjectName))
        {
            var sid = await db.Subjects.Where(s => s.Name == subjectName)
                .Select(s => (int?)s.Id).FirstOrDefaultAsync(ct);
            if (sid.HasValue) query = query.Where(q => q.SubjectId == sid.Value);
        }
        if (unitId.HasValue) query = query.Where(q => q.UnitId == unitId);

        var items = await query.OrderBy(_ => EF.Functions.Random()).Take(count).ToListAsync(ct);
        return Ok(mapper.Map<List<QuestionDto>>(items));
    }

    /// <summary>POST /api/questions/bulk — admin only, idempotent (skips duplicates by ContentHash)</summary>
    [HttpPost("bulk"), Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<BulkCreateResponse>> BulkCreate(
        [FromBody] List<BulkCreateQuestionItem> items, CancellationToken ct)
    {
        // Build subject name → ID cache (case-insensitive)
        var subjectCache = await db.Subjects
            .Select(s => new { s.Name, s.Id })
            .ToDictionaryAsync(s => s.Name.ToLowerInvariant(), s => s.Id, ct);

        // Load existing content hashes to skip duplicates
        var existingHashes = await db.Questions
            .Where(q => q.ContentHash != null)
            .Select(q => q.ContentHash!)
            .ToHashSetAsync(ct);

        int skipped = 0;
        var toInsert = new List<Question>();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Stem)) { skipped++; continue; }

            // Resolve SubjectId from SubjectName if not provided
            int? sid = item.SubjectId;
            if (!sid.HasValue && !string.IsNullOrEmpty(item.SubjectName))
                subjectCache.TryGetValue(item.SubjectName.ToLowerInvariant(), out var resolvedId);
            if (!sid.HasValue && !string.IsNullOrEmpty(item.SubjectName) &&
                subjectCache.TryGetValue(item.SubjectName.ToLowerInvariant(), out var resolved))
                sid = resolved;

            // Compute deduplication hash
            var hash = ComputeHash($"{item.GradeCode}|{sid}|{item.Stem.Trim().ToLowerInvariant()}");
            if (existingHashes.Contains(hash)) { skipped++; continue; }
            existingHashes.Add(hash);

            toInsert.Add(new Question
            {
                Kind           = item.Kind,
                Stem           = item.Stem,
                GradeCode      = item.GradeCode,
                SubjectId      = sid,
                OptionsJson    = item.OptionsJson,
                CorrectIndex   = item.CorrectIndex,
                Difficulty     = item.Difficulty ?? "Medium",
                CognitiveLevel = item.CognitiveLevel ?? "Understanding",
                Feedback       = item.Feedback,
                SloCode        = item.SloCode,
                IsEntranceExam = item.IsEntranceExam,
                IsAiGenerated  = false,
                ContentHash    = hash,
                CreatedById    = cu.UserId,
            });
        }

        if (toInsert.Count > 0)
        {
            db.Questions.AddRange(toInsert);
            await db.SaveChangesAsync(ct);
        }

        return Ok(new BulkCreateResponse(toInsert.Count, skipped, items.Count));
    }

    /// <summary>
    /// PATCH /api/questions/{id}/inline — admin only.
    /// Updates Stem and/or OptionsJson bypassing FluentValidation length limits.
    /// Used by the inline-images migration script to embed base64 images in question content.
    /// </summary>
    [HttpPatch("{id:int}/inline"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> InlinePatch(int id, [FromBody] InlinePatchRequest req, CancellationToken ct)
    {
        var q = await uow.Repository<Question>().FindAsync([id], ct);
        if (q == null) return NotFound();
        if (req.Stem        != null) q.Stem        = req.Stem;
        if (req.OptionsJson != null) q.OptionsJson = req.OptionsJson;
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    private static string ComputeHash(string input)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..16];
    }
}
