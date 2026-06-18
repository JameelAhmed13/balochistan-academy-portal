using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Tests;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/tests")]
[Authorize]
public class TestsController(IApplicationDbContext db, ICurrentUserService cu) : ControllerBase
{
    private static TestDto Map(Test t) => new(
        t.Id, t.Title, t.GradeCode, t.SubjectId, t.UnitId,
        t.Kind, t.DurationMin, t.TotalMarks, t.IsPublished,
        t.TestQuestions.Count, t.CreatedAt);

    /// <summary>GET /api/tests?gradeCode=&amp;kind=&amp;published=true</summary>
    [HttpGet]
    public async Task<IActionResult> GetTests(
        [FromQuery] string? gradeCode, [FromQuery] string? kind,
        [FromQuery] bool published = true, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var q = db.Tests.Include(t => t.TestQuestions).AsQueryable();
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(t => t.GradeCode == gradeCode);
        if (!string.IsNullOrEmpty(kind))      q = q.Where(t => t.Kind == kind);
        if (published)                        q = q.Where(t => t.IsPublished);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { items = items.Select(Map), total, page, pageSize });
    }

    /// <summary>GET /api/tests/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTest(int id, CancellationToken ct)
    {
        var test = await db.Tests
            .Include(t => t.TestQuestions).ThenInclude(tq => tq.Question)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        if (test == null) return NotFound();
        return Ok(new
        {
            test.Id, test.Title, test.GradeCode, test.SubjectId, test.Kind,
            test.DurationMin, test.TotalMarks, test.IsPublished,
            Questions = test.TestQuestions.OrderBy(tq => tq.SortOrder)
                .Select(tq => tq.Question)
        });
    }

    /// <summary>POST /api/tests — admin only</summary>
    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateTestRequest req, CancellationToken ct)
    {
        var test = new Test
        {
            Title = req.Title, Kind = req.Kind,
            GradeCode = req.GradeCode, SubjectId = req.SubjectId,
            UnitId = req.UnitId, DurationMin = req.DurationMin,
            TotalMarks = req.TotalMarks, CreatedById = cu.UserId,
        };
        if (req.QuestionIds?.Length > 0)
        {
            test.TestQuestions = req.QuestionIds.Select((qId, i) =>
                new TestQuestion { QuestionId = qId, SortOrder = i }).ToList();
        }
        db.Tests.Add(test);
        await db.SaveChangesAsync(ct);
        return Created($"/api/tests/{test.Id}", Map(test));
    }

    /// <summary>PATCH /api/tests/{id}/publish — toggle publish</summary>
    [HttpPatch("{id:int}/publish"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> TogglePublish(int id, CancellationToken ct)
    {
        var test = await db.Tests.FindAsync(id, ct);
        if (test == null) return NotFound();
        test.IsPublished = !test.IsPublished;
        await db.SaveChangesAsync(ct);
        return Ok(new { test.Id, test.IsPublished });
    }

    /// <summary>DELETE /api/tests/{id} — admin only</summary>
    [HttpDelete("{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var test = await db.Tests.FindAsync(id, ct);
        if (test == null) return NotFound();
        db.Tests.Remove(test);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Attempts ──────────────────────────────────────────────────────────────

    /// <summary>POST /api/tests/attempts — student submits a test attempt</summary>
    [HttpPost("attempts")]
    public async Task<IActionResult> SubmitAttempt([FromBody] SubmitAttemptRequest req, CancellationToken ct)
    {
        var coins = 0;
        // Earn coins if test has ≥35 questions and it's a qualifying attempt type
        if (req.Total >= 35 && req.AttemptType is "self-test" or "parent-test")
        {
            var alreadyEarned = await db.TestAttempts.AnyAsync(
                a => a.UserId == cu.UserId && a.TestId == req.TestId && a.CoinsEarned > 0, ct);
            if (!alreadyEarned)
            {
                var pct = req.Total > 0 ? (double)req.Score / req.Total : 0;
                coins = pct >= 0.9 ? 50 : pct >= 0.7 ? 30 : pct >= 0.5 ? 15 : 5;
            }
        }

        var attempt = new TestAttempt
        {
            UserId = cu.UserId!.Value, TestId = req.TestId,
            Score = req.Score, Total = req.Total,
            Percent = req.Total > 0 ? (decimal)req.Score / req.Total * 100 : 0,
            DurationSec = req.DurationSec, AnswersJson = req.AnswersJson,
            AttemptType = req.AttemptType, CoinsEarned = coins,
            SubmittedAt = DateTime.UtcNow,
        };
        db.TestAttempts.Add(attempt);
        await db.SaveChangesAsync(ct);

        if (coins > 0)
        {
            db.CoinLedgers.Add(new CoinLedger
            {
                UserId = cu.UserId!.Value, Amount = coins,
                Reason = $"Test completed ({req.AttemptType})", AttemptId = attempt.Id,
            });
            var user = await db.Users.FindAsync(cu.UserId, ct);
            if (user != null) user.Coins += coins;
            await db.SaveChangesAsync(ct);
        }

        return Ok(new AttemptResultDto(attempt.Id, attempt.UserId, attempt.TestId,
            attempt.Score, attempt.Total, attempt.Percent, coins, attempt.AttemptType ?? "", attempt.SubmittedAt));
    }

    /// <summary>GET /api/tests/attempts — student's own attempts</summary>
    [HttpGet("attempts")]
    public async Task<IActionResult> MyAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var q = db.TestAttempts.Where(a => a.UserId == cu.UserId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AttemptResultDto(a.Id, a.UserId, a.TestId, a.Score, a.Total, a.Percent, a.CoinsEarned, a.AttemptType ?? "", a.SubmittedAt))
            .ToListAsync(ct);
        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>GET /api/tests/attempts/{id}</summary>
    [HttpGet("attempts/{id:int}")]
    public async Task<IActionResult> GetAttempt(int id, CancellationToken ct)
    {
        var a = await db.TestAttempts.FindAsync(id, ct);
        if (a == null || a.UserId != cu.UserId) return NotFound();
        return Ok(a);
    }

    /// <summary>GET /api/tests/me/stats — current user summary (attempts, avg, coins)</summary>
    [HttpGet("me/stats")]
    public async Task<IActionResult> MyStats(CancellationToken ct)
    {
        var userId = cu.UserId!.Value;
        var attempts = await db.TestAttempts
            .Where(a => a.UserId == userId)
            .Select(a => new { a.Percent, a.Score, a.Total, a.CoinsEarned })
            .ToListAsync(ct);

        var coins = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Coins)
            .FirstOrDefaultAsync(ct);

        var total   = attempts.Count;
        var passed  = attempts.Count(a => a.Percent >= 40);
        var avg     = total > 0 ? Math.Round(attempts.Average(a => (double)(a.Percent ?? 0)), 1) : 0;
        var earned  = attempts.Sum(a => a.CoinsEarned);

        return Ok(new { attempts = total, avgPercent = avg, passed, coinsEarned = earned, coins });
    }

    /// <summary>GET /api/tests/leaderboard</summary>
    [HttpGet("leaderboard"), AllowAnonymous]
    public async Task<IActionResult> Leaderboard([FromQuery] string? gradeCode, [FromQuery] int top = 20, CancellationToken ct = default)
    {
        var q = db.Users.Include(u => u.Role).Where(u => u.IsActive && u.Role.Name == "student");
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(u => u.GradeCode == gradeCode);

        var results = await q
            .Select(u => new
            {
                u.Id, u.Name, u.GradeCode, u.Coins,
                TotalAttempts = u.TestAttempts.Count,
                AvgPercent    = u.TestAttempts.Any() ? u.TestAttempts.Average(a => (double?)a.Percent) ?? 0 : 0,
            })
            .OrderByDescending(u => u.Coins)
            .Take(top)
            .ToListAsync(ct);

        return Ok(results);
    }
}
