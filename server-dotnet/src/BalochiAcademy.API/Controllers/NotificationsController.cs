using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Notifications;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IApplicationDbContext db, ICurrentUserService cu) : ControllerBase
{
    /// <summary>GET /api/notifications — current user's notifications</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var items = await db.UserNotifications
            .Include(un => un.Notification)
            .Where(un => un.UserId == cu.UserId &&
                         (un.Notification.ExpiresAt == null || un.Notification.ExpiresAt > now))
            .OrderByDescending(un => un.CreatedAt)
            .Select(un => new NotificationDto(
                un.NotificationId, un.Notification.Title, un.Notification.Body,
                un.Notification.Type, un.Notification.Icon, un.IsRead,
                un.Notification.CreatedAt, un.Notification.ExpiresAt))
            .ToListAsync(ct);
        return Ok(items);
    }

    /// <summary>PATCH /api/notifications/{id}/read</summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        var un = await db.UserNotifications
            .FirstOrDefaultAsync(u => u.NotificationId == id && u.UserId == cu.UserId, ct);
        if (un == null) return NotFound();
        un.IsRead = true;
        un.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }

    /// <summary>PATCH /api/notifications/read-all</summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var unread = await db.UserNotifications
            .Where(u => u.UserId == cu.UserId && !u.IsRead).ToListAsync(ct);
        foreach (var u in unread) { u.IsRead = true; u.ReadAt = DateTime.UtcNow; }
        await db.SaveChangesAsync(ct);
        return Ok(new { count = unread.Count });
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>POST /api/admin/notifications — broadcast notification</summary>
    [HttpPost("/api/admin/notifications"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest req, CancellationToken ct)
    {
        var notif = new Notification
        {
            Title = req.Title, Body = req.Body, Type = req.Type, Icon = req.Icon,
            TargetRole = req.TargetRole, TargetGrade = req.TargetGrade,
            CreatedById = cu.UserId, ExpiresAt = req.ExpiresAt,
        };
        db.Notifications.Add(notif);
        await db.SaveChangesAsync(ct);

        // Fan out to matching users
        var userQuery = db.Users.Where(u => u.IsActive);
        if (!string.IsNullOrEmpty(req.TargetRole))
            userQuery = userQuery.Where(u => u.Role.Name == req.TargetRole);
        if (!string.IsNullOrEmpty(req.TargetGrade))
            userQuery = userQuery.Where(u => u.GradeCode == req.TargetGrade);

        var userIds = await userQuery.Select(u => u.Id).ToListAsync(ct);
        db.UserNotifications.AddRange(userIds.Select(uid => new UserNotification
        {
            UserId = uid, NotificationId = notif.Id, CreatedAt = DateTime.UtcNow,
        }));
        await db.SaveChangesAsync(ct);
        return Created($"/api/admin/notifications/{notif.Id}", new { notif.Id, notif.Title, SentTo = userIds.Count });
    }
}
