using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Grades;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api")]
public class CatalogController(IApplicationDbContext db, IAuditService audit, ICurrentUserService cu) : ControllerBase
{
    // ── PUBLIC ────────────────────────────────────────────────────────────────

    /// <summary>GET /api/grades — enabled grades with subject counts</summary>
    [HttpGet("grades")]
    public async Task<IActionResult> GetGrades(CancellationToken ct)
    {
        var grades = await db.Grades
            .Where(g => g.IsEnabled)
            .OrderBy(g => g.SortOrder)
            .Select(g => new
            {
                g.Code, g.Label, g.Band, g.SortOrder, g.IsEnabled,
                SubjectCount = g.GradeSubjects.Count,
            })
            .ToListAsync(ct);
        return Ok(grades);
    }

    /// <summary>GET /api/grades/{code}/subjects</summary>
    [HttpGet("grades/{code}/subjects")]
    public async Task<IActionResult> GetSubjectsForGrade(string code, CancellationToken ct)
    {
        var subjects = await db.GradeSubjects
            .Where(gs => gs.GradeCode == code)
            .Select(gs => gs.Subject)
            .OrderBy(s => s.Id)
            .ToListAsync(ct);
        return Ok(subjects);
    }

    /// <summary>GET /api/grades/{code}/subjects/{subjectId}/syllabus — units→topics→objectives</summary>
    [HttpGet("grades/{code}/subjects/{subjectId:int}/syllabus"), Authorize]
    public async Task<IActionResult> GetSyllabus(string code, int subjectId, CancellationToken ct)
    {
        var units = await db.Units
            .Where(u => u.GradeCode == code && u.SubjectId == subjectId)
            .OrderBy(u => u.SortOrder).ThenBy(u => u.Number)
            .Select(u => new
            {
                u.Id, u.Name, u.Number, u.SortOrder, u.Description,
                Topics = u.Topics.OrderBy(t => t.SortOrder).Select(t => new
                {
                    t.Id, t.Name, t.SortOrder,
                    Objectives = t.Objectives.Select(o => new
                    {
                        o.Id, o.Code, o.ObjectiveText, o.CognitiveLevel
                    }),
                }),
                Objectives = u.Objectives.Where(o => o.TopicId == null).Select(o => new
                {
                    o.Id, o.Code, o.ObjectiveText, o.CognitiveLevel
                }),
            })
            .ToListAsync(ct);
        return Ok(units);
    }

    /// <summary>GET /api/tutors?grade=&amp;subject=</summary>
    [HttpGet("tutors")]
    public async Task<IActionResult> GetTutors([FromQuery] string? grade, [FromQuery] string? subject, CancellationToken ct)
    {
        var q = db.AiTutors.Include(t => t.Subject).Where(t => t.IsActive);
        if (!string.IsNullOrEmpty(grade))
        {
            var subjectIds = await db.GradeSubjects
                .Where(gs => gs.GradeCode == grade).Select(gs => gs.SubjectId).ToListAsync(ct);
            q = q.Where(t => t.GradeCode == null || t.GradeCode == grade)
                 .Where(t => t.SubjectId == null || subjectIds.Contains(t.SubjectId!.Value));
        }
        if (!string.IsNullOrEmpty(subject))
            q = q.Where(t => t.Subject != null && t.Subject.Name == subject);

        return Ok(await q.ToListAsync(ct));
    }

    // ── ADMIN: Grades ─────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/grades — all grades (including disabled)</summary>
    [HttpGet("admin/grades"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminGetGrades(CancellationToken ct)
        => Ok(await db.Grades.OrderBy(g => g.SortOrder)
            .Select(g => new { g.Code, g.Label, g.Band, g.SortOrder, g.IsEnabled, SubjectCount = g.GradeSubjects.Count })
            .ToListAsync(ct));

    /// <summary>POST /api/admin/grades</summary>
    [HttpPost("admin/grades"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest req, CancellationToken ct)
    {
        if (await db.Grades.AnyAsync(g => g.Code == req.Code, ct))
            return Conflict(new { error = "Grade code already exists" });

        var grade = new Grade { Code = req.Code, Label = req.Label, Band = req.Band, SortOrder = req.SortOrder, IsEnabled = req.IsEnabled };
        db.Grades.Add(grade);
        await db.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "Grade", newValues: grade, ct: ct);
        return Created($"/api/admin/grades/{grade.Code}", grade);
    }

    /// <summary>PUT /api/admin/grades/{code}</summary>
    [HttpPut("admin/grades/{code}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateGrade(string code, [FromBody] UpdateGradeRequest req, CancellationToken ct)
    {
        var grade = await db.Grades.FindAsync(new object[] { code }, ct);
        if (grade == null) return NotFound();
        var old = new { grade.Label, grade.Band, grade.SortOrder, grade.IsEnabled };
        if (req.Label     != null) grade.Label     = req.Label;
        if (req.Band      != null) grade.Band      = req.Band;
        if (req.SortOrder != null) grade.SortOrder = req.SortOrder.Value;
        if (req.IsEnabled != null) grade.IsEnabled = req.IsEnabled.Value;
        await db.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "Grade", entityId: 0, oldValues: old, newValues: grade, ct: ct);
        return Ok(grade);
    }

    /// <summary>DELETE /api/admin/grades/{code}</summary>
    [HttpDelete("admin/grades/{code}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteGrade(string code, CancellationToken ct)
    {
        var grade = await db.Grades.FindAsync(new object[] { code }, ct);
        if (grade == null) return NotFound();
        db.Grades.Remove(grade);
        await db.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "Grade", oldValues: grade, ct: ct);
        return NoContent();
    }

    /// <summary>PUT /api/admin/grades/{code}/subjects</summary>
    [HttpPut("admin/grades/{code}/subjects"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetGradeSubjects(string code, [FromBody] SetGradeSubjectsRequest req, CancellationToken ct)
    {
        var existing = await db.GradeSubjects.Where(gs => gs.GradeCode == code).ToListAsync(ct);
        db.GradeSubjects.RemoveRange(existing);
        db.GradeSubjects.AddRange(req.SubjectIds.Select(sid => new GradeSubject { GradeCode = code, SubjectId = sid }));
        await db.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }

    // ── ADMIN: Subjects ───────────────────────────────────────────────────────

    [HttpGet("admin/subjects"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminGetSubjects(CancellationToken ct)
        => Ok(await db.Subjects.OrderBy(s => s.Id).ToListAsync(ct));

    [HttpPost("admin/subjects"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateSubject([FromBody] Subject req, CancellationToken ct)
    {
        if (await db.Subjects.AnyAsync(s => s.Name == req.Name, ct))
            return Conflict(new { error = "Subject name already exists" });
        db.Subjects.Add(req);
        await db.SaveChangesAsync(ct);
        return Created($"/api/admin/subjects/{req.Id}", req);
    }

    [HttpPut("admin/subjects/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] Subject req, CancellationToken ct)
    {
        var subject = await db.Subjects.FindAsync(id, ct);
        if (subject == null) return NotFound();
        subject.Name = req.Name; subject.NameUr = req.NameUr;
        subject.Icon = req.Icon; subject.Color  = req.Color;
        subject.Medium = req.Medium;
        await db.SaveChangesAsync(ct);
        return Ok(subject);
    }

    [HttpDelete("admin/subjects/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteSubject(int id, CancellationToken ct)
    {
        var subject = await db.Subjects.FindAsync(id, ct);
        if (subject == null) return NotFound();
        db.Subjects.Remove(subject);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── ADMIN: AI Tutors ──────────────────────────────────────────────────────

    [HttpPost("admin/tutors"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTutor([FromBody] AiTutor req, CancellationToken ct)
    {
        db.AiTutors.Add(req);
        await db.SaveChangesAsync(ct);
        return Created($"/api/admin/tutors/{req.Id}", req);
    }

    [HttpPut("admin/tutors/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTutor(int id, [FromBody] AiTutor req, CancellationToken ct)
    {
        var t = await db.AiTutors.FindAsync(id, ct);
        if (t == null) return NotFound();
        t.Persona = req.Persona; t.Description = req.Description;
        t.SystemPrompt = req.SystemPrompt; t.IsActive = req.IsActive;
        await db.SaveChangesAsync(ct);
        return Ok(t);
    }

    [HttpDelete("admin/tutors/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTutor(int id, CancellationToken ct)
    {
        var t = await db.AiTutors.FindAsync(id, ct);
        if (t == null) return NotFound();
        db.AiTutors.Remove(t);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Syllabus CRUD (admin) ─────────────────────────────────────────────────

    [HttpPost("admin/syllabus/units"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddUnit([FromBody] Unit req, CancellationToken ct)
    {
        db.Units.Add(req);
        await db.SaveChangesAsync(ct);
        return Created($"/api/admin/syllabus/units/{req.Id}", req);
    }

    [HttpPut("admin/syllabus/units/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUnit(int id, [FromBody] Unit req, CancellationToken ct)
    {
        var u = await db.Units.FindAsync(id, ct);
        if (u == null) return NotFound();
        u.Name = req.Name; u.Number = req.Number;
        u.SortOrder = req.SortOrder; u.Description = req.Description;
        await db.SaveChangesAsync(ct);
        return Ok(u);
    }

    [HttpDelete("admin/syllabus/units/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUnit(int id, CancellationToken ct)
    {
        var u = await db.Units.FindAsync(id, ct);
        if (u == null) return NotFound();
        db.Units.Remove(u);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("admin/syllabus/topics"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddTopic([FromBody] Topic req, CancellationToken ct)
    {
        db.Topics.Add(req);
        await db.SaveChangesAsync(ct);
        return Created($"/api/admin/syllabus/topics/{req.Id}", req);
    }

    [HttpDelete("admin/syllabus/topics/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTopic(int id, CancellationToken ct)
    {
        var t = await db.Topics.FindAsync(id, ct);
        if (t == null) return NotFound();
        db.Topics.Remove(t);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("admin/syllabus/objectives"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddObjective([FromBody] LearningObjective req, CancellationToken ct)
    {
        db.LearningObjectives.Add(req);
        await db.SaveChangesAsync(ct);
        return Created($"/api/admin/syllabus/objectives/{req.Id}", req);
    }

    [HttpDelete("admin/syllabus/objectives/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteObjective(int id, CancellationToken ct)
    {
        var o = await db.LearningObjectives.FindAsync(id, ct);
        if (o == null) return NotFound();
        db.LearningObjectives.Remove(o);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
