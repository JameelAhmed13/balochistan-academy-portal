using BalochiAcademy.API.Hubs;
using BalochiAcademy.Application.Auth;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController(IUnitOfWork uow, ICurrentUserService cu, IPasswordService passwords,
    IHubContext<NotificationHub> hub) : ControllerBase
{
    /// <summary>GET /api/admin/users?role=&amp;gradeCode=&amp;page=</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? role, [FromQuery] string? gradeCode,
        [FromQuery] string? search, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        IQueryable<User> q = uow.Repository<User>().Query()
            .Include(u => u.Role)
            .Include(u => u.Institute);
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
                InstituteId   = (int?)u.InstituteId,
                InstituteName = u.Institute != null ? u.Institute.Name : null,
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

        if (!user.IsActive)
        {
            var active = await uow.Repository<RefreshToken>().Query()
                .Where(rt => rt.UserId == id && rt.RevokedAt == null)
                .ToListAsync(ct);
            foreach (var rt in active) rt.RevokedAt = DateTime.UtcNow;
        }

        await uow.SaveChangesAsync(ct);

        if (!user.IsActive)
            await NotificationHub.SendToUser(hub, id, new { type = "forceLogout" });

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

    /// <summary>POST /api/admin/users — create a new user</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Username and password are required." });

        var exists = await uow.Repository<User>().Query()
            .AnyAsync(u => u.Username == req.Username.Trim().ToLower(), ct);
        if (exists)
            return Conflict(new { message = "Username already exists." });

        var role = await uow.Repository<Role>().Query()
            .FirstOrDefaultAsync(r => r.Name == (req.RoleName ?? "student"), ct);
        if (role == null)
            return BadRequest(new { message = $"Role '{req.RoleName}' not found." });

        var user = new User
        {
            Username     = req.Username.Trim().ToLower(),
            PasswordHash = passwords.Hash(req.Password),
            RoleId       = role.Id,
            Name         = req.Name?.Trim(),
            Phone        = req.Phone?.Trim(),
            Email        = req.Email?.Trim(),
            InstituteId  = req.InstituteId,
            Coins        = req.Coins ?? 0,
            IsActive     = true,
        };
        uow.Repository<User>().Add(user);
        await uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(List), new { id = user.Id }, new
        {
            user.Id, user.Username, user.Name, user.Phone,
            Role = role.Name, user.Coins, user.IsActive, user.CreatedAt,
            InstituteId = user.InstituteId, InstituteName = (string?)null,
        });
    }

    /// <summary>GET /api/admin/users/{id}/sessions — list active sessions</summary>
    [HttpGet("{id:int}/sessions")]
    public async Task<ActionResult<List<SessionDto>>> GetSessions(int id, CancellationToken ct)
    {
        var sessions = await uow.Repository<RefreshToken>().Query()
            .Where(rt => rt.UserId == id && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(rt => rt.CreatedAt)
            .Select(rt => new SessionDto(rt.Id, rt.DeviceName, rt.IpAddress, rt.UserAgent, rt.CreatedAt, rt.ExpiresAt))
            .ToListAsync(ct);
        return Ok(sessions);
    }

    /// <summary>DELETE /api/admin/users/{id}/sessions — revoke all sessions</summary>
    [HttpDelete("{id:int}/sessions")]
    public async Task<IActionResult> RevokeAllSessions(int id, CancellationToken ct)
    {
        var active = await uow.Repository<RefreshToken>().Query()
            .Where(rt => rt.UserId == id && rt.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var rt in active) rt.RevokedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        await NotificationHub.SendToUser(hub, id, new { type = "forceLogout" });
        return Ok(new { count = active.Count });
    }

    /// <summary>DELETE /api/admin/users/{id}/sessions/{tokenId} — revoke a specific session</summary>
    [HttpDelete("{id:int}/sessions/{tokenId:int}")]
    public async Task<IActionResult> RevokeSession(int id, int tokenId, CancellationToken ct)
    {
        var rt = await uow.Repository<RefreshToken>().Query()
            .FirstOrDefaultAsync(rt => rt.Id == tokenId && rt.UserId == id, ct);
        if (rt == null) return NotFound();
        rt.RevokedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        await NotificationHub.SendToUser(hub, id, new { type = "forceLogout" });
        return Ok(new { ok = true });
    }
}

public record CreateUserRequest(
    string   Username,
    string   Password,
    string?  Name        = null,
    string?  Phone       = null,
    string?  Email       = null,
    string?  RoleName    = "student",
    int?     InstituteId = null,
    int?     Coins       = 0
);
