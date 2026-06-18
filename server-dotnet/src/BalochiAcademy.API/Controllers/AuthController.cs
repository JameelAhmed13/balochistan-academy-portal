using BalochiAcademy.Application.Auth;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IApplicationDbContext db,
    ITokenService         tokens,
    IPasswordService      passwords,
    ICurrentUserService   currentUser) : ControllerBase
{
    private UserDto ToDto(User u) => new(
        u.Id, u.Username, u.Role?.Name ?? "student",
        u.Name, u.Phone, u.Email, u.GradeCode,
        u.Medium, u.Institute, u.Board, u.Coins);

    /// <summary>POST /api/auth/login</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == req.Username.Trim() && u.IsActive, ct);

        if (user == null || !passwords.Verify(req.Password, user.PasswordHash))
        {
            db.LoginHistories.Add(new LoginHistory
            {
                UserId = user?.Id, Status = "failed",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await db.SaveChangesAsync(ct);
            return Unauthorized(new { error = "Invalid username or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        var rt = tokens.GenerateRefreshToken(user.Id);
        db.RefreshTokens.Add(rt);
        db.LoginHistories.Add(new LoginHistory
        {
            UserId = user.Id, Status = "ok",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        });
        await db.SaveChangesAsync(ct);

        return Ok(new AuthResponse(tokens.GenerateAccessToken(user), rt.Token, ToDto(user)));
    }

    /// <summary>POST /api/auth/register</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var grade = await db.Grades.FindAsync(new object[] { req.GradeCode }, ct);
        if (grade == null || !grade.IsEnabled)
            return BadRequest(new { error = "Unknown or disabled grade" });

        if (await db.Users.AnyAsync(u => u.Username == req.Username.Trim(), ct))
            return Conflict(new { error = "Username already taken" });

        var studentRole = await db.Roles.FirstAsync(r => r.Name == "student", ct);
        var user = new User
        {
            Username     = req.Username.Trim(),
            PasswordHash = passwords.Hash(req.Password),
            RoleId       = studentRole.Id,
            Name         = req.Name,
            Phone        = req.Phone,
            Email        = req.Email,
            GradeCode    = req.GradeCode,
            Medium       = req.Medium ?? "English",
            Institute    = req.Institute,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        // reload with role navigation
        await db.Users.Entry(user).Reference(u => u.Role).LoadAsync(ct);
        var rt = tokens.GenerateRefreshToken(user.Id);
        db.RefreshTokens.Add(rt);
        await db.SaveChangesAsync(ct);

        return Created($"/api/users/{user.Id}", new AuthResponse(tokens.GenerateAccessToken(user), rt.Token, ToDto(user)));
    }

    /// <summary>GET /api/auth/me</summary>
    [HttpGet("me"), Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId && u.IsActive, ct);
        if (user == null) return Unauthorized();
        return Ok(new { user = ToDto(user) });
    }

    /// <summary>PUT /api/auth/me/grade</summary>
    [HttpPut("me/grade"), Authorize(Policy = "Authenticated")]
    public async Task<ActionResult<UserDto>> UpdateGrade([FromBody] UpdateGradeRequest req, CancellationToken ct)
    {
        var grade = await db.Grades.FindAsync(new object[] { req.GradeCode }, ct);
        if (grade == null || !grade.IsEnabled) return BadRequest(new { error = "Unknown or disabled grade" });

        var user = await db.Users.Include(u => u.Role)
            .FirstAsync(u => u.Id == currentUser.UserId, ct);
        user.GradeCode = req.GradeCode;
        await db.SaveChangesAsync(ct);
        return Ok(new { user = ToDto(user) });
    }

    /// <summary>POST /api/auth/refresh</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        var stored = await db.RefreshTokens
            .Include(rt => rt.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken, ct);

        if (stored == null || !stored.IsActive)
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        // rotate token
        stored.RevokedAt = DateTime.UtcNow;
        var newRt = tokens.GenerateRefreshToken(stored.UserId);
        stored.ReplacedByToken = newRt.Token;
        db.RefreshTokens.Add(newRt);
        await db.SaveChangesAsync(ct);

        return Ok(new AuthResponse(tokens.GenerateAccessToken(stored.User), newRt.Token, ToDto(stored.User)));
    }

    /// <summary>POST /api/auth/logout</summary>
    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        // revoke all active refresh tokens for this user
        var active = await db.RefreshTokens
            .Where(rt => rt.UserId == currentUser.UserId && rt.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var rt in active) rt.RevokedAt = DateTime.UtcNow;

        db.LoginHistories.Add(new LoginHistory
        {
            UserId = currentUser.UserId, Status = "logout",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }
}
