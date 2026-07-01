using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

/// <summary>
/// Read-only curriculum endpoints for authenticated students.
/// Mirrors the admin read endpoints but scoped to published/active data only.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class StudentContentController(IUnitOfWork uow) : ControllerBase
{
    // ── Subjects ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/subjects/{subjectId}/books — active books for a subject (optionally filtered by gradeCode)</summary>
    [HttpGet("subjects/{subjectId:int}/books")]
    public async Task<IActionResult> GetBooksForSubject(
        int subjectId,
        [FromQuery] string? gradeCode,
        CancellationToken ct)
    {
        var q = uow.Repository<Book>().Query()
            .Where(b => b.SubjectId == subjectId && b.IsActive);

        if (!string.IsNullOrEmpty(gradeCode))
            q = q.Where(b => b.GradeCode == null || b.GradeCode == gradeCode);

        var books = await q
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Id)
            .Select(b => new
            {
                b.Id,
                b.SubjectId,
                b.GradeCode,
                b.Title,
                b.TitleUr,
                b.Author,
                b.Publisher,
                b.Edition,
                b.Board,
                b.Medium,
                b.Description,
                b.CoverUrl,
                b.DownloadUrl,
                b.SortOrder,
                UnitCount = b.Units.Count,
            })
            .ToListAsync(ct);

        return Ok(books);
    }

    /// <summary>GET /api/books/{bookId} — single book detail</summary>
    [HttpGet("books/{bookId:int}")]
    public async Task<IActionResult> GetBook(int bookId, CancellationToken ct)
    {
        var book = await uow.Repository<Book>().Query()
            .Where(b => b.Id == bookId && b.IsActive)
            .Select(b => new
            {
                b.Id,
                b.SubjectId,
                b.GradeCode,
                b.Title,
                b.TitleUr,
                b.Author,
                b.Publisher,
                b.Edition,
                b.Board,
                b.Medium,
                b.Description,
                b.CoverUrl,
                b.DownloadUrl,
                b.SortOrder,
                UnitCount = b.Units.Count,
            })
            .FirstOrDefaultAsync(ct);

        if (book == null) return NotFound();
        return Ok(book);
    }

    // ── Books → Units ──────────────────────────────────────────────────────────

    /// <summary>GET /api/books/{bookId}/units — units for a book with topic and resource counts</summary>
    [HttpGet("books/{bookId:int}/units")]
    public async Task<IActionResult> GetUnitsForBook(int bookId, CancellationToken ct)
    {
        // Verify the book exists and is active
        var bookExists = await uow.Repository<Book>().Query()
            .AnyAsync(b => b.Id == bookId && b.IsActive, ct);
        if (!bookExists) return NotFound();

        var units = await uow.Repository<Unit>().Query()
            .Where(u => u.BookId == bookId)
            .OrderBy(u => u.SortOrder).ThenBy(u => u.Number).ThenBy(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.BookId,
                u.SubjectId,
                u.GradeCode,
                u.Name,
                u.Number,
                u.SortOrder,
                u.Description,
                TopicCount    = u.Topics.Count,
                ResourceCount = u.Topics.SelectMany(t => t.Resources).Count(r => r.IsPublished),
            })
            .ToListAsync(ct);

        return Ok(units);
    }

    /// <summary>GET /api/books/{bookId}/syllabus — full syllabus tree: units → topics → objectives</summary>
    [HttpGet("books/{bookId:int}/syllabus")]
    public async Task<IActionResult> GetBookSyllabus(int bookId, CancellationToken ct)
    {
        var bookExists = await uow.Repository<Book>().Query()
            .AnyAsync(b => b.Id == bookId && b.IsActive, ct);
        if (!bookExists) return NotFound();

        var units = await uow.Repository<Unit>().Query()
            .Where(u => u.BookId == bookId)
            .OrderBy(u => u.SortOrder).ThenBy(u => u.Number)
            .Select(u => new
            {
                u.Id,
                u.BookId,
                u.Name,
                u.Number,
                u.SortOrder,
                u.Description,
                Topics = u.Topics.OrderBy(t => t.SortOrder).Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.SortOrder,
                    ResourceCount = t.Resources.Count(r => r.IsPublished),
                    Objectives = t.Objectives.Select(o => new
                    {
                        o.Id,
                        o.Code,
                        o.ObjectiveText,
                        o.CognitiveLevel,
                    }),
                }),
                Objectives = u.Objectives.Where(o => o.TopicId == null).Select(o => new
                {
                    o.Id,
                    o.Code,
                    o.ObjectiveText,
                    o.CognitiveLevel,
                }),
            })
            .ToListAsync(ct);

        return Ok(units);
    }

    // ── Units → Topics ────────────────────────────────────────────────────────

    /// <summary>GET /api/units/{unitId} — single unit detail</summary>
    [HttpGet("units/{unitId:int}")]
    public async Task<IActionResult> GetUnit(int unitId, CancellationToken ct)
    {
        var unit = await uow.Repository<Unit>().Query()
            .Where(u => u.Id == unitId)
            .Select(u => new
            {
                u.Id,
                u.BookId,
                u.SubjectId,
                u.GradeCode,
                u.Name,
                u.Number,
                u.SortOrder,
                u.Description,
                TopicCount    = u.Topics.Count,
                ResourceCount = u.Topics.SelectMany(t => t.Resources).Count(r => r.IsPublished),
            })
            .FirstOrDefaultAsync(ct);

        if (unit == null) return NotFound();
        return Ok(unit);
    }

    /// <summary>GET /api/units/{unitId}/topics — topics for a unit with resource counts</summary>
    [HttpGet("units/{unitId:int}/topics")]
    public async Task<IActionResult> GetTopicsForUnit(int unitId, CancellationToken ct)
    {
        var topics = await uow.Repository<Topic>().Query()
            .Where(t => t.UnitId == unitId)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Id)
            .Select(t => new
            {
                t.Id,
                t.UnitId,
                t.Name,
                t.SortOrder,
                ResourceCount = t.Resources.Count(r => r.IsPublished),
            })
            .ToListAsync(ct);

        return Ok(topics);
    }

    /// <summary>GET /api/units/{unitId}/resources — published content items for a unit</summary>
    [HttpGet("units/{unitId:int}/resources")]
    public async Task<IActionResult> GetResourcesForUnit(int unitId, CancellationToken ct)
    {
        var items = await uow.Repository<ContentItem>().Query()
            .Where(c => c.UnitId == unitId && c.IsPublished)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.UnitId,
                c.TopicId,
                c.SubjectId,
                c.GradeCode,
                c.Kind,
                c.Title,
                c.Description,
                c.Url,
                c.ThumbnailUrl,
                c.DurationSec,
                c.Tags,
                c.SortOrder,
                c.SourceYear,
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    // ── Topics → Resources ────────────────────────────────────────────────────

    /// <summary>GET /api/topics/{topicId} — single topic detail</summary>
    [HttpGet("topics/{topicId:int}")]
    public async Task<IActionResult> GetTopic(int topicId, CancellationToken ct)
    {
        var topic = await uow.Repository<Topic>().Query()
            .Where(t => t.Id == topicId)
            .Select(t => new
            {
                t.Id,
                t.UnitId,
                t.Name,
                t.SortOrder,
                ResourceCount = t.Resources.Count(r => r.IsPublished),
                Objectives = t.Objectives.Select(o => new
                {
                    o.Id,
                    o.Code,
                    o.ObjectiveText,
                    o.CognitiveLevel,
                }),
            })
            .FirstOrDefaultAsync(ct);

        if (topic == null) return NotFound();
        return Ok(topic);
    }

    /// <summary>GET /api/topics/{topicId}/resources — published content items for a topic</summary>
    [HttpGet("topics/{topicId:int}/resources")]
    public async Task<IActionResult> GetResourcesForTopic(int topicId, CancellationToken ct)
    {
        var items = await uow.Repository<ContentItem>().Query()
            .Where(c => c.TopicId == topicId && c.IsPublished)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.TopicId,
                c.UnitId,
                c.SubjectId,
                c.GradeCode,
                c.Kind,
                c.Title,
                c.Description,
                c.Url,
                c.ThumbnailUrl,
                c.DurationSec,
                c.Tags,
                c.SortOrder,
                c.SourceYear,
            })
            .ToListAsync(ct);

        return Ok(items);
    }

}
