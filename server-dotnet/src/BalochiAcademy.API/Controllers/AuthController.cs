using AutoMapper;
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
    IUnitOfWork           uow,
    ITokenService         tokens,
    IPasswordService      passwords,
    ICurrentUserService   currentUser,
    IMapper               mapper) : ControllerBase
{
    /// <summary>POST /api/auth/login</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await uow.Repository<User>().Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == req.Username.Trim() && u.IsActive, ct);

        if (user == null || !passwords.Verify(req.Password, user.PasswordHash))
        {
            uow.Repository<LoginHistory>().Add(new LoginHistory
            {
                UserId    = user?.Id,
                Status    = "failed",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            await uow.SaveChangesAsync(ct);
            return Unauthorized(new { error = "Invalid username or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        var rt = tokens.GenerateRefreshToken(user.Id);
        uow.Repository<RefreshToken>().Add(rt);
        uow.Repository<LoginHistory>().Add(new LoginHistory
        {
            UserId    = user.Id,
            Status    = "ok",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
        });
        await uow.SaveChangesAsync(ct);

        return Ok(new AuthResponse(tokens.GenerateAccessToken(user), rt.Token, mapper.Map<UserDto>(user)));
    }

    /// <summary>POST /api/auth/register</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var grade = await uow.Repository<Grade>().FindAsync([req.GradeCode], ct);
        if (grade == null || !grade.IsEnabled)
            return BadRequest(new { error = "Unknown or disabled grade" });

        if (await uow.Repository<User>().Query().AnyAsync(u => u.Username == req.Username.Trim(), ct))
            return Conflict(new { error = "Username already taken" });

        var studentRole = await uow.Repository<Role>().Query().FirstAsync(r => r.Name == "student", ct);
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
        uow.Repository<User>().Add(user);
        await uow.SaveChangesAsync(ct);

        // reload with role navigation
        user = await uow.Repository<User>().Query()
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == user.Id, ct);

        var rt = tokens.GenerateRefreshToken(user.Id);
        uow.Repository<RefreshToken>().Add(rt);
        await uow.SaveChangesAsync(ct);

        return Created($"/api/users/{user.Id}",
            new AuthResponse(tokens.GenerateAccessToken(user), rt.Token, mapper.Map<UserDto>(user)));
    }

    /// <summary>GET /api/auth/me</summary>
    [HttpGet("me"), Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var user = await uow.Repository<User>().Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId && u.IsActive, ct);
        if (user == null) return Unauthorized();
        return Ok(new { user = mapper.Map<UserDto>(user) });
    }

    /// <summary>PUT /api/auth/me/grade</summary>
    [HttpPut("me/grade"), Authorize(Policy = "Authenticated")]
    public async Task<ActionResult<UserDto>> UpdateGrade([FromBody] UpdateGradeRequest req, CancellationToken ct)
    {
        var grade = await uow.Repository<Grade>().FindAsync([req.GradeCode], ct);
        if (grade == null || !grade.IsEnabled) return BadRequest(new { error = "Unknown or disabled grade" });

        var user = await uow.Repository<User>().Query()
            .Include(u => u.Role)
            .FirstAsync(u => u.Id == currentUser.UserId, ct);
        user.GradeCode = req.GradeCode;
        await uow.SaveChangesAsync(ct);
        return Ok(new { user = mapper.Map<UserDto>(user) });
    }

    /// <summary>POST /api/auth/refresh</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        var stored = await uow.Repository<RefreshToken>().Query()
            .Include(rt => rt.User).ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken, ct);

        if (stored == null || !stored.IsActive)
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        stored.RevokedAt = DateTime.UtcNow;
        var newRt = tokens.GenerateRefreshToken(stored.UserId);
        stored.ReplacedByToken = newRt.Token;
        uow.Repository<RefreshToken>().Add(newRt);
        await uow.SaveChangesAsync(ct);

        return Ok(new AuthResponse(tokens.GenerateAccessToken(stored.User), newRt.Token, mapper.Map<UserDto>(stored.User)));
    }

    /// <summary>POST /api/auth/logout</summary>
    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var active = await uow.Repository<RefreshToken>().Query()
            .Where(rt => rt.UserId == currentUser.UserId && rt.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var rt in active) rt.RevokedAt = DateTime.UtcNow;

        uow.Repository<LoginHistory>().Add(new LoginHistory
        {
            UserId    = currentUser.UserId,
            Status    = "logout",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        });
        await uow.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }
}
