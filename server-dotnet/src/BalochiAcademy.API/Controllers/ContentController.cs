using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public class ContentController(IUnitOfWork uow, ICurrentUserService cu) : ControllerBase
{
    /// <summary>GET /api/content?gradeCode=&amp;subjectId=&amp;kind=</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? gradeCode, [FromQuery] int? subjectId,
        [FromQuery] string? kind, [FromQuery] int? unitId,
        CancellationToken ct = default)
    {
        var q = uow.Repository<ContentItem>().Query().Where(c => c.IsPublished);
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(c => c.GradeCode == gradeCode);
        if (subjectId.HasValue)               q = q.Where(c => c.SubjectId == subjectId);
        if (!string.IsNullOrEmpty(kind))      q = q.Where(c => c.Kind == kind);
        if (unitId.HasValue)                  q = q.Where(c => c.UnitId == unitId);
        return Ok(await q.OrderBy(c => c.SortOrder).ToListAsync(ct));
    }

    /// <summary>POST /api/admin/content — admin uploads content item</summary>
    [HttpPost("/api/admin/content"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] ContentItem req, CancellationToken ct)
    {
        uow.Repository<ContentItem>().Add(req);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/content/{req.Id}", req);
    }

    /// <summary>PUT /api/admin/content/{id}</summary>
    [HttpPut("/api/admin/content/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] ContentItem req, CancellationToken ct)
    {
        var item = await uow.Repository<ContentItem>().FindAsync([id], ct);
        if (item == null) return NotFound();
        item.Title = req.Title; item.Description = req.Description;
        item.Url = req.Url; item.ThumbnailUrl = req.ThumbnailUrl;
        item.IsPublished = req.IsPublished; item.SortOrder = req.SortOrder;
        item.Tags = req.Tags;
        await uow.SaveChangesAsync(ct);
        return Ok(item);
    }

    /// <summary>DELETE /api/admin/content/{id}</summary>
    [HttpDelete("/api/admin/content/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await uow.Repository<ContentItem>().FindAsync([id], ct);
        if (item == null) return NotFound();
        uow.Repository<ContentItem>().Remove(item);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Video Courses ──────────────────────────────────────────────────────────

    /// <summary>GET /api/content/courses?gradeCode=&amp;subjectId=</summary>
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] string? gradeCode, [FromQuery] int? subjectId, CancellationToken ct = default)
    {
        var q = uow.Repository<VideoCourse>().Query().Include(c => c.Lessons).Where(c => c.IsPublished);
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(c => c.GradeCode == gradeCode);
        if (subjectId.HasValue)               q = q.Where(c => c.SubjectId == subjectId);
        return Ok(await q.OrderBy(c => c.SortOrder).Select(c => new
        {
            c.Id, c.Title, c.Description, c.ThumbnailUrl, c.TutorName,
            LessonCount = c.Lessons.Count,
        }).ToListAsync(ct));
    }

    /// <summary>GET /api/content/courses/{id}</summary>
    [HttpGet("courses/{id:int}")]
    public async Task<IActionResult> GetCourse(int id, CancellationToken ct)
    {
        var course = await uow.Repository<VideoCourse>().Query()
            .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course == null) return NotFound();
        return Ok(course);
    }

    // ── Student Notes ──────────────────────────────────────────────────────────

    /// <summary>GET /api/content/notes</summary>
    [HttpGet("notes")]
    public async Task<IActionResult> GetNotes(CancellationToken ct)
        => Ok(await uow.Repository<StudentNote>().Query()
            .Where(n => n.UserId == cu.UserId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(ct));

    /// <summary>POST /api/content/notes</summary>
    [HttpPost("notes")]
    public async Task<IActionResult> CreateNote([FromBody] StudentNote req, CancellationToken ct)
    {
        req.UserId = cu.UserId!.Value;
        uow.Repository<StudentNote>().Add(req);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/content/notes/{req.Id}", req);
    }

    /// <summary>PUT /api/content/notes/{id}</summary>
    [HttpPut("notes/{id:int}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] StudentNote req, CancellationToken ct)
    {
        var note = await uow.Repository<StudentNote>().Query()
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == cu.UserId, ct);
        if (note == null) return NotFound();
        note.Title = req.Title; note.Content = req.Content;
        note.Tags = req.Tags; note.UpdatedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        return Ok(note);
    }

    /// <summary>DELETE /api/content/notes/{id}</summary>
    [HttpDelete("notes/{id:int}")]
    public async Task<IActionResult> DeleteNote(int id, CancellationToken ct)
    {
        var note = await uow.Repository<StudentNote>().Query()
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == cu.UserId, ct);
        if (note == null) return NotFound();
        uow.Repository<StudentNote>().Remove(note);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
