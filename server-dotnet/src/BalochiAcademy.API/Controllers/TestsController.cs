using AutoMapper;
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
public class TestsController(IUnitOfWork uow, ICurrentUserService cu, IMapper mapper, ISystemSettingsService settings, IAuditService audit) : ControllerBase
{
    /// <summary>GET /api/tests?gradeCode=&amp;kind=&amp;unitId=&amp;subjectId=&amp;published=true</summary>
    [HttpGet]
    public async Task<IActionResult> GetTests(
        [FromQuery] string? gradeCode, [FromQuery] string? kind,
        [FromQuery] int? unitId, [FromQuery] int? subjectId,
        [FromQuery] bool published = true, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        IQueryable<Test> q = uow.Repository<Test>().Query().Include(t => t.TestQuestions);
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(t => t.GradeCode == gradeCode);
        if (!string.IsNullOrEmpty(kind))      q = q.Where(t => t.Kind == kind);
        if (unitId.HasValue)                  q = q.Where(t => t.UnitId == unitId);
        if (subjectId.HasValue)               q = q.Where(t => t.SubjectId == subjectId);
        if (published)                        q = q.Where(t => t.IsPublished);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { items = mapper.Map<List<TestDto>>(items), totalCount = total, page, pageSize });
    }

    /// <summary>GET /api/tests/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTest(int id, CancellationToken ct)
    {
        var test = await uow.Repository<Test>().Query()
            .Include(t => t.TestQuestions).ThenInclude(tq => tq.Question)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        if (test == null) return NotFound();
        return Ok(new
        {
            test.Id, test.Title, test.GradeCode, test.SubjectId, test.Kind,
            test.DurationMin, test.TotalMarks, test.IsPublished,
            Questions = test.TestQuestions.OrderBy(tq => tq.SortOrder).Select(tq => new {
                tq.Question.Id,
                tq.Question.Kind,
                tq.Question.Stem,
                tq.Question.GradeCode,
                tq.Question.SubjectId,
                tq.Question.Difficulty,
                tq.Question.Marks,
                tq.Question.IsAiGenerated,
                tq.Question.OptionsJson,
                tq.Question.CorrectIndex,
                tq.Question.ModelAnswer,
                tq.Question.Feedback,
            }),
        });
    }

    /// <summary>POST /api/tests — admin only</summary>
    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateTestRequest req, CancellationToken ct)
    {
        var test = new Test
        {
            Title       = req.Title,      Kind        = req.Kind,
            GradeCode   = req.GradeCode,  SubjectId   = req.SubjectId,
            UnitId      = req.UnitId,     DurationMin = req.DurationMin,
            TotalMarks  = req.TotalMarks, IsScheduled = req.IsScheduled,
            ScheduledAt = req.ScheduledAt, EndsAt     = req.EndsAt,
            CreatedById = cu.UserId,
        };
        if (req.QuestionIds?.Length > 0)
        {
            test.TestQuestions = req.QuestionIds
                .Select((qId, i) => new TestQuestion { QuestionId = qId, SortOrder = i })
                .ToList();
        }
        uow.Repository<Test>().Add(test);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "Test", test.Id, newValues: new { test.Title }, ip: cu.IpAddress, ct: ct);
        return Created($"/api/tests/{test.Id}", mapper.Map<TestDto>(test));
    }

    /// <summary>PUT /api/tests/{id} — admin only</summary>
    [HttpPut("{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTestRequest req, CancellationToken ct)
    {
        var test = await uow.Repository<Test>().Query()
            .Include(t => t.TestQuestions)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        if (test == null) return NotFound();

        if (req.Title != null) test.Title    = req.Title;
        if (req.Kind  != null) test.Kind     = req.Kind;
        test.GradeCode   = req.GradeCode;
        test.SubjectId   = req.SubjectId;
        test.UnitId      = req.UnitId;
        test.DurationMin = req.DurationMin;
        test.TotalMarks  = req.TotalMarks;
        test.IsScheduled = req.IsScheduled;
        test.ScheduledAt = req.ScheduledAt;
        test.EndsAt      = req.EndsAt;

        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "Test", id, newValues: new { test.Title }, ip: cu.IpAddress, ct: ct);
        return Ok(mapper.Map<TestDto>(test));
    }

    /// <summary>PUT /api/tests/{id}/questions — replace question list</summary>
    [HttpPut("{id:int}/questions"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetQuestions(int id, [FromBody] int[] questionIds, CancellationToken ct)
    {
        var test = await uow.Repository<Test>().Query()
            .Include(t => t.TestQuestions)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        if (test == null) return NotFound();

        test.TestQuestions.Clear();
        foreach (var (qId, i) in questionIds.Select((qId, i) => (qId, i)))
            test.TestQuestions.Add(new TestQuestion { QuestionId = qId, SortOrder = i });

        await uow.SaveChangesAsync(ct);
        return Ok(new { testId = id, questionCount = test.TestQuestions.Count });
    }

    /// <summary>PATCH /api/tests/{id}/publish — toggle publish</summary>
    [HttpPatch("{id:int}/publish"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> TogglePublish(int id, CancellationToken ct)
    {
        var test = await uow.Repository<Test>().FindAsync([id], ct);
        if (test == null) return NotFound();
        test.IsPublished = !test.IsPublished;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, test.IsPublished ? "Publish" : "Unpublish", "Test", id,
            newValues: new { test.Title }, ip: cu.IpAddress, ct: ct);
        return Ok(new { test.Id, test.IsPublished });
    }

    /// <summary>DELETE /api/tests/{id} — admin only</summary>
    [HttpDelete("{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var test = await uow.Repository<Test>().FindAsync([id], ct);
        if (test == null) return NotFound();
        uow.Repository<Test>().Remove(test);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "Test", id, oldValues: new { test.Title }, ip: cu.IpAddress, ct: ct);
        return NoContent();
    }

    // ── Attempts ─────────────────────────────────────────────────────────────

    /// <summary>POST /api/tests/attempts — student submits a test attempt</summary>
    [HttpPost("attempts")]
    public async Task<IActionResult> SubmitAttempt([FromBody] SubmitAttemptRequest req, CancellationToken ct)
    {
        var coins = 0;
        if (req.Total >= 35 && req.AttemptType is "self-test" or "parent-test" or "daily" or "monthly" or "challenge" or "weekly")
        {
            bool alreadyEarned;
            if (req.TestId.HasValue)
            {
                alreadyEarned = await uow.Repository<TestAttempt>().Query().AnyAsync(
                    a => a.UserId == cu.UserId && a.TestId == req.TestId && a.CoinsEarned > 0, ct);
            }
            else
            {
                // Self-test/daily/monthly/challenge/weekly attempts are generated on the fly and have no
                // TestId to dedupe against — cap coin-earning to once per day per attempt type so the same
                // generated test can't be replayed for infinite coins.
                var todayStart = DateTime.UtcNow.Date;
                alreadyEarned = await uow.Repository<TestAttempt>().Query().AnyAsync(
                    a => a.UserId == cu.UserId && a.AttemptType == req.AttemptType
                      && a.CoinsEarned > 0 && a.SubmittedAt >= todayStart, ct);
            }
            if (!alreadyEarned)
            {
                var pct  = req.Total > 0 ? (double)req.Score / req.Total : 0;
                var c90  = int.Parse(await settings.GetAsync("coins_per_90pct", "50",  ct) ?? "50");
                var c70  = int.Parse(await settings.GetAsync("coins_per_70pct", "30",  ct) ?? "30");
                var c50  = int.Parse(await settings.GetAsync("coins_per_50pct", "15",  ct) ?? "15");
                var pass = int.Parse(await settings.GetAsync("coins_per_pass",  "5",   ct) ?? "5");
                coins = pct >= 0.9 ? c90 : pct >= 0.7 ? c70 : pct >= 0.5 ? c50 : pass;
            }
        }

        var attempt = new TestAttempt
        {
            UserId      = cu.UserId!.Value, TestId      = req.TestId,
            SubjectId   = req.SubjectId,
            Score       = req.Score,        Total       = req.Total,
            Percent     = req.Total > 0 ? (decimal)req.Score / req.Total * 100 : 0,
            DurationSec = req.DurationSec,  AnswersJson = req.AnswersJson,
            AttemptType = req.AttemptType,  CoinsEarned = coins,
            SubmittedAt = DateTime.UtcNow,
        };
        uow.Repository<TestAttempt>().Add(attempt);
        await uow.SaveChangesAsync(ct);

        if (coins > 0)
        {
            uow.Repository<CoinLedger>().Add(new CoinLedger
            {
                UserId = cu.UserId!.Value, Amount = coins,
                Reason = $"Test completed ({req.AttemptType})", AttemptId = attempt.Id,
            });
            var user = await uow.Repository<User>().FindAsync([cu.UserId], ct);
            if (user != null) user.Coins += coins;
            await uow.SaveChangesAsync(ct);
        }

        var result = (await AttachSubjectNames([mapper.Map<AttemptResultDto>(attempt)], ct))[0];
        return Ok(result);
    }

    /// <summary>GET /api/tests/attempts — student's own attempts</summary>
    [HttpGet("attempts")]
    public async Task<IActionResult> MyAttempts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var q = uow.Repository<TestAttempt>().Query().Where(a => a.UserId == cu.UserId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        var dtos = await AttachSubjectNames(mapper.Map<List<AttemptResultDto>>(items), ct);
        return Ok(new { items = dtos, total, page, pageSize });
    }

    /// <summary>GET /api/tests/attempts/{id}</summary>
    [HttpGet("attempts/{id:int}")]
    public async Task<IActionResult> GetAttempt(int id, CancellationToken ct)
    {
        var a = await uow.Repository<TestAttempt>().FindAsync([id], ct);
        if (a == null || a.UserId != cu.UserId) return NotFound();
        var result = (await AttachSubjectNames([mapper.Map<AttemptResultDto>(a)], ct))[0];
        return Ok(result);
    }

    /// <summary>Batch-fills SubjectName by looking up the distinct SubjectIds in one query — AttemptResultDto
    /// has no Subject navigation to map directly, since TestAttempt.SubjectId is a bare column, not a real FK.</summary>
    private async Task<List<AttemptResultDto>> AttachSubjectNames(List<AttemptResultDto> dtos, CancellationToken ct)
    {
        var subjectIds = dtos.Where(d => d.SubjectId.HasValue).Select(d => d.SubjectId!.Value).Distinct().ToList();
        if (subjectIds.Count == 0) return dtos;
        var names = await uow.Repository<Subject>().Query()
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);
        return dtos.Select(d => d.SubjectId.HasValue && names.TryGetValue(d.SubjectId.Value, out var name)
            ? d with { SubjectName = name } : d).ToList();
    }

    /// <summary>GET /api/tests/me/stats</summary>
    [HttpGet("me/stats")]
    public async Task<IActionResult> MyStats(CancellationToken ct)
    {
        var userId = cu.UserId!.Value;
        var attempts = await uow.Repository<TestAttempt>().Query()
            .Where(a => a.UserId == userId)
            .Select(a => new { a.Percent, a.Score, a.Total, a.CoinsEarned, a.SubmittedAt })
            .ToListAsync(ct);

        var coins = await uow.Repository<User>().Query()
            .Where(u => u.Id == userId)
            .Select(u => u.Coins)
            .FirstOrDefaultAsync(ct);

        var now        = DateTime.UtcNow;
        var total      = attempts.Count;
        var passed     = attempts.Count(a => a.Percent >= 40);
        var avg        = total > 0 ? Math.Round(attempts.Average(a => (double)(a.Percent ?? 0)), 1) : 0;
        var earned     = attempts.Sum(a => a.CoinsEarned);
        var passRate   = total > 0 ? Math.Round((double)passed / total * 100, 1) : 0;
        var thisMonth  = attempts.Count(a => a.SubmittedAt.Month == now.Month && a.SubmittedAt.Year == now.Year);

        return Ok(new { attempts = total, avgPercent = avg, passed, passRate, thisMonth, coinsEarned = earned, coins });
    }

    /// <summary>GET /api/tests/leaderboard</summary>
    [HttpGet("leaderboard"), AllowAnonymous]
    public async Task<IActionResult> Leaderboard(
        [FromQuery] string? gradeCode, [FromQuery] int top = 20, CancellationToken ct = default)
    {
        var q = uow.Repository<User>().Query()
            .Include(u => u.Role)
            .Where(u => u.IsActive && u.Role.Name == "student");
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(u => u.GradeCode == gradeCode);

        var results = await q
            .Select(u => new
            {
                u.Id, u.Name, u.GradeCode, u.Coins,
                TotalAttempts = u.TestAttempts.Count,
                AvgPercent    = u.TestAttempts.Any()
                    ? u.TestAttempts.Average(a => (double?)a.Percent) ?? 0 : 0,
            })
            .OrderByDescending(u => u.Coins)
            .Take(top)
            .ToListAsync(ct);

        return Ok(results);
    }
}
