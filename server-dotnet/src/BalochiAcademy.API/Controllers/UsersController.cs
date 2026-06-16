using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController(IUnitOfWork uow, ICurrentUserService cu) : ControllerBase
{
    /// <summary>GET /api/admin/users?role=&amp;gradeCode=&amp;page=</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? role, [FromQuery] string? gradeCode,
        [FromQuery] string? search, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        IQueryable<User> q = uow.Repository<User>().Query().Include(u => u.Role);
        if (!string.IsNullOrEmpty(role))      q = q.Where(u => u.Role.Name == role);
        if (!string.IsNullOrEmpty(gradeCode)) q = q.Where(u => u.GradeCode == gradeCode);
        if (!string.IsNullOrEmpty(search))    q = q.Where(u => u.Name!.Contains(search) || u.Username.Contains(search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(u => new
            {
                u.Id, u.Username, u.Name, u.Phone, u.Email,
                Role = u.Role.Name, u.GradeCode, u.Medium,
                u.Coins, u.IsActive, u.CreatedAt, u.LastLoginAt,
            })
            .ToListAsync(ct);
        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>GET /api/admin/users/{id}/results — student's test attempts</summary>
    [HttpGet("{id:int}/results")]
    public async Task<IActionResult> StudentResults(
        int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var q = uow.Repository<TestAttempt>().Query().Where(a => a.UserId == id);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.Id, a.TestId, a.Score, a.Total, a.Percent,
                a.CoinsEarned, a.AttemptType, a.DurationSec, a.SubmittedAt,
            })
            .ToListAsync(ct);
        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>PATCH /api/admin/users/{id}/toggle — activate/deactivate</summary>
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id, CancellationToken ct)
    {
        var user = await uow.Repository<User>().FindAsync([id], ct);
        if (user == null) return NotFound();
        user.IsActive = !user.IsActive;
        await uow.SaveChangesAsync(ct);
        return Ok(new { user.Id, user.IsActive });
    }

    /// <summary>GET /api/admin/analytics — dashboard KPIs</summary>
    [HttpGet("/api/admin/analytics")]
    public async Task<IActionResult> Analytics(CancellationToken ct)
    {
        var totalStudents  = await uow.Repository<User>().Query().CountAsync(u => u.Role.Name == "student" && u.IsActive, ct);
        var totalQuestions = await uow.Repository<Question>().Query().CountAsync(ct);
        var totalTests     = await uow.Repository<Test>().Query().CountAsync(ct);
        var totalAttempts  = await uow.Repository<TestAttempt>().Query().CountAsync(ct);
        var totalCoins     = await uow.Repository<User>().Query().SumAsync(u => (long)u.Coins, ct);

        var byGrade = await uow.Repository<User>().Query()
            .Where(u => u.Role.Name == "student" && u.IsActive && u.GradeCode != null)
            .GroupBy(u => u.GradeCode)
            .Select(g => new { grade = g.Key, count = g.Count() })
            .ToListAsync(ct);

        return Ok(new { totalStudents, totalQuestions, totalTests, totalAttempts, totalCoins, byGrade });
    }

    /// <summary>GET /api/admin/settings</summary>
    [HttpGet("/api/admin/settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
        => Ok(await uow.Repository<SystemSetting>().Query()
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct));

    /// <summary>PUT /api/admin/settings/{key}</summary>
    [HttpPut("/api/admin/settings/{key}")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] string value, CancellationToken ct)
    {
        var setting = await uow.Repository<SystemSetting>().FindAsync([key], ct);
        if (setting == null) return NotFound();
        setting.Value       = value;
        setting.UpdatedAt   = DateTime.UtcNow;
        setting.UpdatedById = cu.UserId;
        await uow.SaveChangesAsync(ct);
        return Ok(new { key, value });
    }
}
