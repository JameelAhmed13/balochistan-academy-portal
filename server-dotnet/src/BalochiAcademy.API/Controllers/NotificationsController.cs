using BalochiAcademy.API.Hubs;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Notifications;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IUnitOfWork uow, ICurrentUserService cu, IHubContext<NotificationHub> hub, IAuditService audit) : ControllerBase
{
    /// <summary>GET /api/notifications — current user's notifications</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var items = await uow.Repository<UserNotification>().Query()
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
        var un = await uow.Repository<UserNotification>().Query()
            .FirstOrDefaultAsync(u => u.NotificationId == id && u.UserId == cu.UserId, ct);
        if (un == null) return NotFound();
        un.IsRead = true;
        un.ReadAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        return Ok(new { ok = true });
    }

    /// <summary>PATCH /api/notifications/read-all</summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var unread = await uow.Repository<UserNotification>().Query()
            .Where(u => u.UserId == cu.UserId && !u.IsRead).ToListAsync(ct);
        foreach (var u in unread) { u.IsRead = true; u.ReadAt = DateTime.UtcNow; }
        await uow.SaveChangesAsync(ct);
        return Ok(new { count = unread.Count });
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>POST /api/admin/notifications — broadcast notification</summary>
    [HttpPost("/api/admin/notifications"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest req, CancellationToken ct)
    {
        var notif = new Notification
        {
            Title       = req.Title,      Body        = req.Body,
            Type        = req.Type,       Icon        = req.Icon,
            TargetRole  = req.TargetRole, TargetGrade = req.TargetGrade,
            CreatedById = cu.UserId,      ExpiresAt   = req.ExpiresAt,
        };
        uow.Repository<Notification>().Add(notif);
        await uow.SaveChangesAsync(ct);

        var userQuery = uow.Repository<User>().Query().Where(u => u.IsActive);
        if (!string.IsNullOrEmpty(req.TargetRole))
            userQuery = userQuery.Where(u => u.Role.Name == req.TargetRole);
        if (!string.IsNullOrEmpty(req.TargetGrade))
            userQuery = userQuery.Where(u => u.GradeCode == req.TargetGrade);

        var userIds = await userQuery.Select(u => u.Id).ToListAsync(ct);
        uow.Repository<UserNotification>().AddRange(userIds.Select(uid => new UserNotification
        {
            UserId = uid, NotificationId = notif.Id, CreatedAt = DateTime.UtcNow,
        }));
        await uow.SaveChangesAsync(ct);

        // Real-time push via SignalR
        var payload = new NotificationDto(
            notif.Id, notif.Title, notif.Body, notif.Type, notif.Icon,
            false, notif.CreatedAt, notif.ExpiresAt);

        if (string.IsNullOrEmpty(req.TargetRole) && string.IsNullOrEmpty(req.TargetGrade))
            await NotificationHub.Broadcast(hub, payload);
        else
            await Task.WhenAll(userIds.Select(uid => NotificationHub.SendToUser(hub, uid, payload)));

        await audit.LogAsync(cu.UserId, "Send", "Notification", notif.Id,
            newValues: new { notif.Title, req.TargetRole, req.TargetGrade, SentTo = userIds.Count }, ip: cu.IpAddress, ct: ct);

        return Created($"/api/admin/notifications/{notif.Id}",
            new { notif.Id, notif.Title, SentTo = userIds.Count });
    }

    // ── Templates ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/notification-templates</summary>
    [HttpGet("/api/admin/notification-templates"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetTemplates(CancellationToken ct)
    {
        var items = await uow.Repository<NotificationTemplate>().Query()
            .OrderBy(t => t.IsDefault ? 0 : 1).ThenBy(t => t.Id)
            .Select(t => new { t.Id, t.Title, t.Body, t.Type, t.Icon, t.IsDefault, t.CreatedAt })
            .ToListAsync(ct);
        return Ok(items);
    }

    /// <summary>POST /api/admin/notification-templates</summary>
    [HttpPost("/api/admin/notification-templates"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateTemplate([FromBody] NotificationTemplateRequest req, CancellationToken ct)
    {
        var t = new NotificationTemplate
        {
            Title = req.Title, Body = req.Body,
            Type  = req.Type ?? "info", Icon = req.Icon,
        };
        uow.Repository<NotificationTemplate>().Add(t);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "NotificationTemplate", t.Id, newValues: new { t.Title }, ip: cu.IpAddress, ct: ct);
        return Created($"/api/admin/notification-templates/{t.Id}",
            new { t.Id, t.Title, t.Body, t.Type, t.Icon, t.IsDefault, t.CreatedAt });
    }

    /// <summary>DELETE /api/admin/notification-templates/{id}</summary>
    [HttpDelete("/api/admin/notification-templates/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTemplate(int id, CancellationToken ct)
    {
        var t = await uow.Repository<NotificationTemplate>().Query()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (t == null) return NotFound();
        if (t.IsDefault) return BadRequest(new { error = "Default templates cannot be deleted." });
        uow.Repository<NotificationTemplate>().Remove(t);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "NotificationTemplate", id, oldValues: new { t.Title }, ip: cu.IpAddress, ct: ct);
        return Ok(new { ok = true });
    }

    /// <summary>GET /api/admin/notifications — paginated sent notification history</summary>
    [HttpGet("/api/admin/notifications"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = uow.Repository<Notification>().Query()
            .Include(n => n.UserNotifications)
            .OrderByDescending(n => n.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new {
                n.Id, n.Title, n.Body, n.Type, n.Icon,
                n.TargetRole, n.TargetGrade, n.CreatedAt, n.ExpiresAt,
                SentTo    = n.UserNotifications.Count,
                ReadCount = n.UserNotifications.Count(un => un.IsRead),
            })
            .ToListAsync(ct);
        return Ok(new { total, items });
    }
}
