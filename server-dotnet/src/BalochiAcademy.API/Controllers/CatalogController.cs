using AutoMapper;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Grades;
using BalochiAcademy.Application.Questions;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api")]
public class CatalogController(IUnitOfWork uow, IAuditService audit, ICurrentUserService cu, ISystemSettingsService settings, IMapper mapper) : ControllerBase
{
    // ── PUBLIC ────────────────────────────────────────────────────────────────

    /// <summary>GET /api/mediums — all teaching mediums ordered by sort order</summary>
    [HttpGet("mediums")]
    public async Task<IActionResult> GetMediums(CancellationToken ct)
        => Ok(await uow.Repository<Medium>().Query()
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Name)
            .Select(m => new { m.Id, m.Name, m.Label, m.SortOrder })
            .ToListAsync(ct));

    /// <summary>GET /api/bands — all grade bands ordered by sort order</summary>
    [HttpGet("bands")]
    public async Task<IActionResult> GetBands(CancellationToken ct)
        => Ok(await uow.Repository<GradeBand>().Query()
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Name)
            .Select(b => new { b.Id, b.Name, b.SortOrder })
            .ToListAsync(ct));

    /// <summary>GET /api/grades — enabled grades with subject counts</summary>
    [HttpGet("grades")]
    public async Task<IActionResult> GetGrades(CancellationToken ct)
    {
        try
        {
            var grades = await uow.Repository<Grade>().Query()
               .Where(g => g.IsEnabled)
               .OrderBy(g => g.SortOrder)
               .Select(g => new
               {
                   g.Code,
                   g.Label,
                   g.Band,
                   g.SortOrder,
                   g.IsEnabled,
                   SubjectCount = g.GradeSubjects.Count,
               })
               .ToListAsync(ct);

            var allowedRaw = await settings.GetAsync("allowed_grades", "", ct);
            if (!string.IsNullOrWhiteSpace(allowedRaw))
            {
                var allowed = allowedRaw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
                grades = grades.Where(g => allowed.Contains(g.Code)).ToList();
            }

            return Ok(grades);
        }
        catch (Exception ex)
        {

            return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>GET /api/grades/{code}/subjects</summary>
    [HttpGet("grades/{code}/subjects")]
    public async Task<IActionResult> GetSubjectsForGrade(string code, CancellationToken ct)
    {
        var subjects = await uow.Repository<GradeSubject>().Query()
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
        var units = await uow.Repository<Unit>().Query()
            .Where(u => u.GradeCode == code && u.SubjectId == subjectId)
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
                    Objectives = t.Objectives.Select(o => new
                    {
                        o.Id,
                        o.Code,
                        o.ObjectiveText,
                        o.CognitiveLevel
                    }),
                }),
                Objectives = u.Objectives.Where(o => o.TopicId == null).Select(o => new
                {
                    o.Id,
                    o.Code,
                    o.ObjectiveText,
                    o.CognitiveLevel
                }),
            })
            .ToListAsync(ct);
        return Ok(units);
    }

    /// <summary>GET /api/tutors?grade=&amp;subject=</summary>
    [HttpGet("tutors")]
    public async Task<IActionResult> GetTutors(
        [FromQuery] string? grade, [FromQuery] string? subject, CancellationToken ct)
    {
        var q = uow.Repository<AiTutor>().Query().Include(t => t.Subject).Where(t => t.IsActive);
        if (!string.IsNullOrEmpty(grade))
        {
            var subjectIds = await uow.Repository<GradeSubject>().Query()
                .Where(gs => gs.GradeCode == grade).Select(gs => gs.SubjectId).ToListAsync(ct);
            q = q.Where(t => t.GradeCode == null || t.GradeCode == grade)
                 .Where(t => t.SubjectId == null || subjectIds.Contains(t.SubjectId!.Value));
        }
        if (!string.IsNullOrEmpty(subject))
            q = q.Where(t => t.Subject != null && t.Subject.Name == subject);

        var result = await q.Select(t => new
        {
            t.Id,
            t.SubjectId,
            t.GradeCode,
            t.Persona,
            t.Slug,
            t.Icon,
            t.Color,
            t.Description,
            t.IsActive,
            Subject = t.Subject == null ? null : new
            {
                t.Subject.Id,
                t.Subject.Name,
                t.Subject.NameUr,
                t.Subject.Icon,
                t.Subject.Color,
                t.Subject.Medium,
            },
        }).ToListAsync(ct);
        return Ok(result);
    }

    // ── Past Papers ───────────────────────────────────────────────────────────

    /// <summary>GET /api/past-papers?subjectId=&amp;gradeCode= — returns all active past papers for a subject/grade.</summary>
    [HttpGet("past-papers")]
    public async Task<IActionResult> GetPastPapers(
        [FromQuery] int?    subjectId,
        [FromQuery] string? gradeCode,
        CancellationToken   ct)
    {
        var q = uow.Repository<PastPaper>().Query().Where(p => p.IsActive);
        if (subjectId.HasValue)              q = q.Where(p => p.SubjectId == subjectId.Value);
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(p => p.GradeCode == gradeCode);

        var papers = await q
            .OrderByDescending(p => p.Year)
            .ThenBy(p => p.PaperType)
            .ThenBy(p => p.SortOrder)
            .Select(p => new
            {
                p.Id, p.Year, p.Board, p.PaperType,
                p.TotalMarks, p.TimeLimitMinutes,
                SubjectName = p.Subject!.Name,
            })
            .ToListAsync(ct);

        return Ok(papers);
    }

    /// <summary>GET /api/past-papers/{id}/questions — the real questions tagged to this specific paper, in paper order.
    /// Returns an empty array (not 404) if the paper exists but has no questions tagged yet — callers should fall
    /// back to a general subject/grade question pool in that case rather than treating it as an error.</summary>
    [HttpGet("past-papers/{id:int}/questions")]
    public async Task<IActionResult> GetPastPaperQuestions(int id, CancellationToken ct)
    {
        var paperExists = await uow.Repository<PastPaper>().Query().AnyAsync(p => p.Id == id && p.IsActive, ct);
        if (!paperExists) return NotFound();

        var questions = await uow.Repository<Question>().Query()
            .Where(q => q.PastPaperId == id)
            .OrderBy(q => q.Id)
            .ToListAsync(ct);

        return Ok(mapper.Map<List<QuestionDto>>(questions));
    }

    // ── ADMIN: Mediums ────────────────────────────────────────────────────────

    [HttpPost("admin/mediums"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateMedium([FromBody] Medium req, CancellationToken ct)
    {
        if (await uow.Repository<Medium>().Query().AnyAsync(m => m.Name == req.Name, ct))
            return Conflict(new { error = "Medium name already exists" });
        uow.Repository<Medium>().Add(req);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "Medium", newValues: req, ct: ct);
        return Created($"/api/admin/mediums/{req.Id}", new { req.Id, req.Name, req.Label, req.SortOrder });
    }

    [HttpPut("admin/mediums/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateMedium(int id, [FromBody] Medium req, CancellationToken ct)
    {
        var medium = await uow.Repository<Medium>().FindAsync([id], ct);
        if (medium == null) return NotFound();
        medium.Name      = req.Name;
        medium.Label     = req.Label;
        medium.SortOrder = req.SortOrder;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "Medium", entityId: id, newValues: medium, ct: ct);
        return Ok(new { medium.Id, medium.Name, medium.Label, medium.SortOrder });
    }

    [HttpDelete("admin/mediums/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteMedium(int id, CancellationToken ct)
    {
        var medium = await uow.Repository<Medium>().FindAsync([id], ct);
        if (medium == null) return NotFound();
        uow.Repository<Medium>().Remove(medium);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "Medium", oldValues: medium, ct: ct);
        return NoContent();
    }

    // ── ADMIN: Grade Bands ────────────────────────────────────────────────────

    [HttpPost("admin/bands"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateBand([FromBody] GradeBand req, CancellationToken ct)
    {
        if (await uow.Repository<GradeBand>().Query().AnyAsync(b => b.Name == req.Name, ct))
            return Conflict(new { error = "Band name already exists" });
        uow.Repository<GradeBand>().Add(req);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "GradeBand", newValues: req, ct: ct);
        return Created($"/api/admin/bands/{req.Id}", new { req.Id, req.Name, req.SortOrder });
    }

    [HttpPut("admin/bands/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateBand(int id, [FromBody] GradeBand req, CancellationToken ct)
    {
        var band = await uow.Repository<GradeBand>().FindAsync([id], ct);
        if (band == null) return NotFound();
        band.Name      = req.Name;
        band.SortOrder = req.SortOrder;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "GradeBand", entityId: id, newValues: band, ct: ct);
        return Ok(new { band.Id, band.Name, band.SortOrder });
    }

    [HttpDelete("admin/bands/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteBand(int id, CancellationToken ct)
    {
        var band = await uow.Repository<GradeBand>().FindAsync([id], ct);
        if (band == null) return NotFound();
        uow.Repository<GradeBand>().Remove(band);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "GradeBand", oldValues: band, ct: ct);
        return NoContent();
    }

    // ── ADMIN: Grades ─────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/grades — all grades (including disabled)</summary>
    [HttpGet("admin/grades"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminGetGrades(CancellationToken ct)
        => Ok(await uow.Repository<Grade>().Query()
            .OrderBy(g => g.SortOrder)
            .Select(g => new { g.Code, g.Label, g.Band, g.SortOrder, g.IsEnabled, SubjectCount = g.GradeSubjects.Count })
            .ToListAsync(ct));

    /// <summary>POST /api/admin/grades</summary>
    [HttpPost("admin/grades"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest req, CancellationToken ct)
    {
        if (await uow.Repository<Grade>().Query().AnyAsync(g => g.Code == req.Code, ct))
            return Conflict(new { error = "Grade code already exists" });

        var grade = new Grade
        {
            Code = req.Code,
            Label = req.Label,
            Band = req.Band,
            SortOrder = req.SortOrder,
            IsEnabled = req.IsEnabled,
        };
        uow.Repository<Grade>().Add(grade);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "Grade", newValues: grade, ct: ct);
        return Created($"/api/admin/grades/{grade.Code}", grade);
    }

    /// <summary>PUT /api/admin/grades/{code}</summary>
    [HttpPut("admin/grades/{code}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateGrade(string code, [FromBody] UpdateGradeRequest req, CancellationToken ct)
    {
        var grade = await uow.Repository<Grade>().FindAsync([code], ct);
        if (grade == null) return NotFound();
        var old = new { grade.Label, grade.Band, grade.SortOrder, grade.IsEnabled };
        if (req.Label != null) grade.Label = req.Label;
        if (req.Band != null) grade.Band = req.Band;
        if (req.SortOrder != null) grade.SortOrder = req.SortOrder.Value;
        if (req.IsEnabled != null) grade.IsEnabled = req.IsEnabled.Value;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "Grade", entityId: 0, oldValues: old, newValues: grade, ct: ct);
        return Ok(grade);
    }

    /// <summary>DELETE /api/admin/grades/{code}</summary>
    [HttpDelete("admin/grades/{code}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteGrade(string code, CancellationToken ct)
    {
        var grade = await uow.Repository<Grade>().FindAsync([code], ct);
        if (grade == null) return NotFound();
        uow.Repository<Grade>().Remove(grade);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "Grade", oldValues: grade, ct: ct);
        return NoContent();
    }

    /// <summary>PUT /api/admin/grades/{code}/subjects</summary>
    [HttpPut("admin/grades/{code}/subjects"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetGradeSubjects(string code, [FromBody] SetGradeSubjectsRequest req, CancellationToken ct)
    {
        var existing = await uow.Repository<GradeSubject>().Query()
            .Where(gs => gs.GradeCode == code).ToListAsync(ct);
        uow.Repository<GradeSubject>().RemoveRange(existing);
        uow.Repository<GradeSubject>().AddRange(
            req.SubjectIds.Select(sid => new GradeSubject { GradeCode = code, SubjectId = sid }));
        await uow.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }

    // ── ADMIN: Subjects ───────────────────────────────────────────────────────

    [HttpGet("admin/subjects"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminGetSubjects(CancellationToken ct)
        => Ok(await uow.Repository<Subject>().Query()
            .OrderBy(s => s.Id)
            .Select(s => new
            {
                s.Id, s.Name, s.NameUr, s.Icon, s.Color, s.Medium, s.CreatedAt,
                BookCount = s.Books.Count,
            })
            .ToListAsync(ct));

    [HttpPost("admin/subjects"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateSubject([FromBody] Subject req, CancellationToken ct)
    {
        if (await uow.Repository<Subject>().Query().AnyAsync(s => s.Name == req.Name, ct))
            return Conflict(new { error = "Subject name already exists" });
        uow.Repository<Subject>().Add(req);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/admin/subjects/{req.Id}", req);
    }

    [HttpPut("admin/subjects/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] Subject req, CancellationToken ct)
    {
        var subject = await uow.Repository<Subject>().FindAsync([id], ct);
        if (subject == null) return NotFound();
        subject.Name = req.Name; subject.NameUr = req.NameUr;
        subject.Icon = req.Icon; subject.Color = req.Color;
        subject.Medium = req.Medium;
        await uow.SaveChangesAsync(ct);
        return Ok(subject);
    }

    [HttpDelete("admin/subjects/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteSubject(int id, CancellationToken ct)
    {
        var subject = await uow.Repository<Subject>().FindAsync([id], ct);
        if (subject == null) return NotFound();
        uow.Repository<Subject>().Remove(subject);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── ADMIN: AI Tutors ──────────────────────────────────────────────────────

    // ── ADMIN: Books ──────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/subjects/{subjectId}/grades — grades linked to this subject</summary>
    [HttpGet("admin/subjects/{subjectId:int}/grades"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetGradesForSubject(int subjectId, CancellationToken ct)
    {
        var grades = await uow.Repository<GradeSubject>().Query()
            .Where(gs => gs.SubjectId == subjectId)
            .OrderBy(gs => gs.Grade.SortOrder)
            .Select(gs => new { gs.Grade.Code, gs.Grade.Label, gs.Grade.SortOrder })
            .ToListAsync(ct);
        return Ok(grades);
    }

    /// <summary>GET /api/admin/subjects/{subjectId}/books — all books for a subject</summary>
    [HttpGet("admin/subjects/{subjectId:int}/books"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetBooks(int subjectId, CancellationToken ct)
        => Ok(await uow.Repository<Book>().Query()
            .Where(b => b.SubjectId == subjectId)
            .OrderBy(b => b.SortOrder).ThenBy(b => b.Id)
            .Select(b => new
            {
                b.Id, b.SubjectId, b.GradeCode,
                b.Title, b.TitleUr, b.Author, b.Publisher,
                b.Edition, b.Board, b.Medium,
                b.Description, b.CoverUrl, b.DownloadUrl,
                b.SortOrder, b.IsActive, b.CreatedAt,
                UnitCount = b.Units.Count,
            })
            .ToListAsync(ct));

    /// <summary>POST /api/admin/subjects/{subjectId}/books</summary>
    [HttpPost("admin/subjects/{subjectId:int}/books"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateBook(int subjectId, [FromBody] Book req, CancellationToken ct)
    {
        req.SubjectId = subjectId;
        uow.Repository<Book>().Add(req);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "Book", newValues: req, ct: ct);
        return Created($"/api/admin/subjects/{subjectId}/books/{req.Id}", new
        {
            req.Id, req.SubjectId, req.GradeCode,
            req.Title, req.TitleUr, req.Author, req.Publisher,
            req.Edition, req.Board, req.Medium,
            req.Description, req.CoverUrl, req.DownloadUrl,
            req.SortOrder, req.IsActive,
        });
    }

    /// <summary>PUT /api/admin/subjects/{subjectId}/books/{id}</summary>
    [HttpPut("admin/subjects/{subjectId:int}/books/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateBook(int subjectId, int id, [FromBody] Book req, CancellationToken ct)
    {
        var book = await uow.Repository<Book>().Query()
            .FirstOrDefaultAsync(b => b.Id == id && b.SubjectId == subjectId, ct);
        if (book == null) return NotFound();
        book.GradeCode   = req.GradeCode;
        book.Title       = req.Title;
        book.TitleUr     = req.TitleUr;
        book.Author      = req.Author;
        book.Publisher   = req.Publisher;
        book.Edition     = req.Edition;
        book.Board       = req.Board;
        book.Medium      = req.Medium;
        book.Description = req.Description;
        book.CoverUrl    = req.CoverUrl;
        book.DownloadUrl = req.DownloadUrl;
        book.SortOrder   = req.SortOrder;
        book.IsActive    = req.IsActive;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "Book", entityId: id, newValues: book, ct: ct);
        return Ok(new
        {
            book.Id, book.SubjectId, book.GradeCode,
            book.Title, book.TitleUr, book.Author, book.Publisher,
            book.Edition, book.Board, book.Medium,
            book.Description, book.CoverUrl, book.DownloadUrl,
            book.SortOrder, book.IsActive,
        });
    }

    /// <summary>DELETE /api/admin/subjects/{subjectId}/books/{id}</summary>
    [HttpDelete("admin/subjects/{subjectId:int}/books/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteBook(int subjectId, int id, CancellationToken ct)
    {
        var book = await uow.Repository<Book>().Query()
            .FirstOrDefaultAsync(b => b.Id == id && b.SubjectId == subjectId, ct);
        if (book == null) return NotFound();
        uow.Repository<Book>().Remove(book);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "Book", oldValues: book, ct: ct);
        return NoContent();
    }

    // ── ADMIN: Book Units ─────────────────────────────────────────────────────

    /// <summary>GET /api/admin/books/{bookId}/units</summary>
    [HttpGet("admin/books/{bookId:int}/units"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetBookUnits(int bookId, CancellationToken ct)
        => Ok(await uow.Repository<Unit>().Query()
            .Where(u => u.BookId == bookId)
            .OrderBy(u => u.SortOrder).ThenBy(u => u.Number).ThenBy(u => u.Id)
            .Select(u => new
            {
                u.Id, u.BookId, u.SubjectId, u.GradeCode,
                u.Name, u.Number, u.SortOrder, u.Description, u.Source, u.CreatedAt,
                TopicCount = u.Topics.Count,
            })
            .ToListAsync(ct));

    /// <summary>GET /api/admin/books/{bookId}/syllabus — book units → topics → objectives</summary>
    [HttpGet("admin/books/{bookId:int}/syllabus"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetBookSyllabus(int bookId, CancellationToken ct)
    {
        var units = await uow.Repository<Unit>().Query()
            .Where(u => u.BookId == bookId)
            .OrderBy(u => u.SortOrder).ThenBy(u => u.Number)
            .Select(u => new
            {
                u.Id, u.BookId, u.Name, u.Number, u.SortOrder, u.Description,
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

    /// <summary>POST /api/admin/books/{bookId}/units</summary>
    [HttpPost("admin/books/{bookId:int}/units"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateBookUnit(int bookId, [FromBody] Unit req, CancellationToken ct)
    {
        var book = await uow.Repository<Book>().FindAsync([bookId], ct);
        if (book == null) return NotFound(new { error = "Book not found" });
        req.BookId    = bookId;
        req.SubjectId = book.SubjectId;
        req.GradeCode = book.GradeCode ?? string.Empty;
        uow.Repository<Unit>().Add(req);
        await uow.SaveChangesAsync(ct);
        var unitSnap = new { req.Id, req.BookId, req.SubjectId, req.GradeCode, req.Name, req.Number, req.SortOrder, req.Description, req.Source };
        await audit.LogAsync(cu.UserId, "Create", "Unit", newValues: unitSnap, ct: ct);
        return Created($"/api/admin/books/{bookId}/units/{req.Id}", unitSnap);
    }

    /// <summary>PUT /api/admin/books/{bookId}/units/{id}</summary>
    [HttpPut("admin/books/{bookId:int}/units/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateBookUnit(int bookId, int id, [FromBody] Unit req, CancellationToken ct)
    {
        var u = await uow.Repository<Unit>().Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.BookId == bookId, ct);
        if (u == null) return NotFound();
        u.Name        = req.Name;
        u.Number      = req.Number;
        u.SortOrder   = req.SortOrder;
        u.Description = req.Description;
        u.Source      = req.Source;
        await uow.SaveChangesAsync(ct);
        var snap = new { u.Id, u.BookId, u.SubjectId, u.GradeCode, u.Name, u.Number, u.SortOrder, u.Description, u.Source };
        await audit.LogAsync(cu.UserId, "Update", "Unit", entityId: id, newValues: snap, ct: ct);
        return Ok(snap);
    }

    /// <summary>DELETE /api/admin/books/{bookId}/units/{id}</summary>
    [HttpDelete("admin/books/{bookId:int}/units/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteBookUnit(int bookId, int id, CancellationToken ct)
    {
        var u = await uow.Repository<Unit>().Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.BookId == bookId, ct);
        if (u == null) return NotFound();
        var delSnap = new { u.Id, u.BookId, u.SubjectId, u.GradeCode, u.Name, u.Number, u.SortOrder };
        uow.Repository<Unit>().Remove(u);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "Unit", oldValues: delSnap, ct: ct);
        return NoContent();
    }

    [HttpGet("admin/tutors"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllTutors(CancellationToken ct)
    {
        var tutors = await uow.Repository<AiTutor>().Query()
            .OrderBy(t => t.Persona)
            .Select(t => new {
                t.Id, t.SubjectId, t.GradeCode, t.Persona, t.Slug,
                t.Icon, t.Color, t.Description, t.SystemPrompt, t.IsActive,
                SubjectName = t.Subject == null ? null : t.Subject.Name,
            })
            .ToListAsync(ct);
        return Ok(tutors);
    }

    [HttpPost("admin/tutors"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTutor([FromBody] AiTutor req, CancellationToken ct)
    {
        uow.Repository<AiTutor>().Add(req);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/admin/tutors/{req.Id}",
            new { req.Id, req.SubjectId, req.GradeCode, req.Persona, req.Slug,
                  req.Icon, req.Color, req.Description, req.SystemPrompt, req.IsActive });
    }

    [HttpPut("admin/tutors/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTutor(int id, [FromBody] AiTutor req, CancellationToken ct)
    {
        var t = await uow.Repository<AiTutor>().FindAsync([id], ct);
        if (t == null) return NotFound();
        t.Persona      = req.Persona;
        t.SubjectId    = req.SubjectId;
        t.GradeCode    = req.GradeCode;
        t.Icon         = req.Icon;
        t.Color        = req.Color;
        t.Description  = req.Description;
        t.SystemPrompt = req.SystemPrompt;
        t.IsActive     = req.IsActive;
        await uow.SaveChangesAsync(ct);
        return Ok(new { t.Id, t.SubjectId, t.GradeCode, t.Persona, t.Slug,
                        t.Icon, t.Color, t.Description, t.SystemPrompt, t.IsActive });
    }

    [HttpDelete("admin/tutors/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTutor(int id, CancellationToken ct)
    {
        var t = await uow.Repository<AiTutor>().FindAsync([id], ct);
        if (t == null) return NotFound();
        uow.Repository<AiTutor>().Remove(t);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── ADMIN: Unit Topics ────────────────────────────────────────────────────

    /// <summary>GET /api/admin/units/{unitId}/topics</summary>
    [HttpGet("admin/units/{unitId:int}/topics"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUnitTopics(int unitId, CancellationToken ct)
        => Ok(await uow.Repository<Topic>().Query()
            .Where(t => t.UnitId == unitId)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Id)
            .Select(t => new {
                t.Id, t.UnitId, t.Name, t.SortOrder, t.CreatedAt,
                ResourceCount = t.Resources.Count,
            })
            .ToListAsync(ct));

    /// <summary>POST /api/admin/units/{unitId}/topics</summary>
    [HttpPost("admin/units/{unitId:int}/topics"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateUnitTopic(int unitId, [FromBody] Topic req, CancellationToken ct)
    {
        req.UnitId = unitId;
        uow.Repository<Topic>().Add(req);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "Topic", newValues: new { req.Id, req.UnitId, req.Name, req.SortOrder }, ct: ct);
        return Created($"/api/admin/units/{unitId}/topics/{req.Id}", new { req.Id, req.UnitId, req.Name, req.SortOrder });
    }

    /// <summary>PUT /api/admin/units/{unitId}/topics/{id}</summary>
    [HttpPut("admin/units/{unitId:int}/topics/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUnitTopic(int unitId, int id, [FromBody] Topic req, CancellationToken ct)
    {
        var t = await uow.Repository<Topic>().Query()
            .FirstOrDefaultAsync(t => t.Id == id && t.UnitId == unitId, ct);
        if (t == null) return NotFound();
        t.Name      = req.Name;
        t.SortOrder = req.SortOrder;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "Topic", entityId: id, newValues: new { t.Id, t.UnitId, t.Name, t.SortOrder }, ct: ct);
        return Ok(new { t.Id, t.UnitId, t.Name, t.SortOrder });
    }

    /// <summary>DELETE /api/admin/units/{unitId}/topics/{id}</summary>
    [HttpDelete("admin/units/{unitId:int}/topics/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUnitTopic(int unitId, int id, CancellationToken ct)
    {
        var t = await uow.Repository<Topic>().Query()
            .FirstOrDefaultAsync(t => t.Id == id && t.UnitId == unitId, ct);
        if (t == null) return NotFound();
        uow.Repository<Topic>().Remove(t);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "Topic", oldValues: new { t.Id, t.UnitId, t.Name }, ct: ct);
        return NoContent();
    }

    // ── ADMIN: Topic Resources (ContentItems per Topic) ───────────────────────

    /// <summary>GET /api/admin/topics/{topicId}/resources</summary>
    [HttpGet("admin/topics/{topicId:int}/resources"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetTopicResources(int topicId, CancellationToken ct)
        => Ok(await uow.Repository<ContentItem>().Query()
            .Where(c => c.TopicId == topicId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => new {
                c.Id, c.TopicId, c.UnitId, c.SubjectId, c.GradeCode,
                c.Kind, c.Title, c.Description,
                c.Url, c.ThumbnailUrl, c.DurationSec, c.Tags,
                c.SortOrder, c.IsPublished, c.SourceYear, c.CreatedAt,
            })
            .ToListAsync(ct));

    /// <summary>POST /api/admin/topics/{topicId}/resources</summary>
    [HttpPost("admin/topics/{topicId:int}/resources"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTopicResource(int topicId, [FromBody] ContentItem req, CancellationToken ct)
    {
        var topic = await uow.Repository<Topic>().FindAsync([topicId], ct);
        if (topic == null) return NotFound(new { error = "Topic not found." });

        var unit = await uow.Repository<Unit>().FindAsync([topic.UnitId], ct);

        req.TopicId   = topicId;
        req.UnitId    = topic.UnitId;
        req.SubjectId = unit?.SubjectId;
        req.GradeCode = unit?.GradeCode;

        // Clear nav props to prevent EF tracking / circular reference issues
        req.Topic = null; req.Unit = null; req.Subject = null; req.Grade = null; req.CreatedBy = null;

        uow.Repository<ContentItem>().Add(req);
        await uow.SaveChangesAsync(ct);

        var snap = new { req.Id, req.TopicId, req.UnitId, req.Kind, req.Title, req.Url, req.SortOrder, req.IsPublished };
        await audit.LogAsync(cu.UserId, "Create", "ContentItem", newValues: snap, ct: ct);
        return Created($"/api/admin/topics/{topicId}/resources/{req.Id}", snap);
    }

    /// <summary>PUT /api/admin/topics/{topicId}/resources/{id}</summary>
    [HttpPut("admin/topics/{topicId:int}/resources/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTopicResource(int topicId, int id, [FromBody] ContentItem req, CancellationToken ct)
    {
        var item = await uow.Repository<ContentItem>().Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.TopicId == topicId, ct);
        if (item == null) return NotFound();

        item.Title        = req.Title;
        item.Kind         = req.Kind;
        item.Description  = req.Description;
        item.Url          = req.Url;
        item.ThumbnailUrl = req.ThumbnailUrl;
        item.DurationSec  = req.DurationSec;
        item.SortOrder    = req.SortOrder;
        item.IsPublished  = req.IsPublished;
        item.Tags         = req.Tags;
        item.SourceYear   = req.SourceYear;

        await uow.SaveChangesAsync(ct);

        var snap = new { item.Id, item.TopicId, item.Kind, item.Title, item.Url, item.SortOrder, item.IsPublished };
        await audit.LogAsync(cu.UserId, "Update", "ContentItem", entityId: id, newValues: snap, ct: ct);
        return Ok(snap);
    }

    /// <summary>DELETE /api/admin/topics/{topicId}/resources/{id}</summary>
    [HttpDelete("admin/topics/{topicId:int}/resources/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTopicResource(int topicId, int id, CancellationToken ct)
    {
        var item = await uow.Repository<ContentItem>().Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.TopicId == topicId, ct);
        if (item == null) return NotFound();

        var snap = new { item.Id, item.TopicId, item.Kind, item.Title };
        uow.Repository<ContentItem>().Remove(item);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "ContentItem", oldValues: snap, ct: ct);
        return NoContent();
    }

    // ── ADMIN: Unit Resources (ContentItems) ──────────────────────────────────

    /// <summary>GET /api/admin/units/{unitId}/resources — all items incl. unpublished</summary>
    [HttpGet("admin/units/{unitId:int}/resources"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetUnitResources(int unitId, CancellationToken ct)
        => Ok(await uow.Repository<ContentItem>().Query()
            .Where(c => c.UnitId == unitId)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Id)
            .Select(c => new
            {
                c.Id, c.UnitId, c.Kind, c.Title, c.Description,
                c.Url, c.ThumbnailUrl, c.DurationSec, c.Tags,
                c.SortOrder, c.IsPublished, c.SourceYear, c.CreatedAt,
            })
            .ToListAsync(ct));

    // ── Syllabus CRUD (admin) ─────────────────────────────────────────────────

    [HttpPost("admin/syllabus/units"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddUnit([FromBody] Unit req, CancellationToken ct)
    {
        uow.Repository<Unit>().Add(req);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/admin/syllabus/units/{req.Id}", req);
    }

    [HttpPut("admin/syllabus/units/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateUnit(int id, [FromBody] Unit req, CancellationToken ct)
    {
        var u = await uow.Repository<Unit>().FindAsync([id], ct);
        if (u == null) return NotFound();
        u.Name = req.Name; u.Number = req.Number;
        u.SortOrder = req.SortOrder; u.Description = req.Description;
        await uow.SaveChangesAsync(ct);
        return Ok(u);
    }

    [HttpDelete("admin/syllabus/units/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUnit(int id, CancellationToken ct)
    {
        var u = await uow.Repository<Unit>().FindAsync([id], ct);
        if (u == null) return NotFound();
        uow.Repository<Unit>().Remove(u);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("admin/syllabus/topics"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddTopic([FromBody] Topic req, CancellationToken ct)
    {
        uow.Repository<Topic>().Add(req);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/admin/syllabus/topics/{req.Id}", req);
    }

    [HttpPut("admin/syllabus/topics/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTopic(int id, [FromBody] Topic req, CancellationToken ct)
    {
        var t = await uow.Repository<Topic>().FindAsync([id], ct);
        if (t == null) return NotFound();
        t.Name = req.Name;
        t.SortOrder = req.SortOrder;
        await uow.SaveChangesAsync(ct);
        return Ok(new { t.Id, t.UnitId, t.Name, t.SortOrder });
    }

    [HttpDelete("admin/syllabus/topics/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTopic(int id, CancellationToken ct)
    {
        var t = await uow.Repository<Topic>().FindAsync([id], ct);
        if (t == null) return NotFound();
        uow.Repository<Topic>().Remove(t);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("admin/syllabus/objectives"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddObjective([FromBody] LearningObjective req, CancellationToken ct)
    {
        uow.Repository<LearningObjective>().Add(req);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/admin/syllabus/objectives/{req.Id}",
            new { req.Id, req.UnitId, req.TopicId, req.ObjectiveText, req.Code, req.CognitiveLevel });
    }

    [HttpPut("admin/syllabus/objectives/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateObjective(int id, [FromBody] LearningObjective req, CancellationToken ct)
    {
        var o = await uow.Repository<LearningObjective>().FindAsync([id], ct);
        if (o == null) return NotFound();
        o.ObjectiveText = req.ObjectiveText;
        o.Code = req.Code;
        o.CognitiveLevel = req.CognitiveLevel;
        await uow.SaveChangesAsync(ct);
        return Ok(new { o.Id, o.UnitId, o.TopicId, o.ObjectiveText, o.Code, o.CognitiveLevel });
    }

    [HttpDelete("admin/syllabus/objectives/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteObjective(int id, CancellationToken ct)
    {
        var o = await uow.Repository<LearningObjective>().FindAsync([id], ct);
        if (o == null) return NotFound();
        uow.Repository<LearningObjective>().Remove(o);
        await uow.SaveChangesAsync(ct);
        return NoContent();
    }
}
