using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

/// <summary>
/// Read-only browsing of the admin action audit trail — who did what, to what, when, and from
/// where. Entries are written by <see cref="IAuditService"/> from the controllers that perform
/// privileged actions (users, tests, subscriptions, complaints, notifications, roles,
/// institutes, settings, catalog).
/// </summary>
[ApiController]
[Route("api/admin/audit-log")]
[Authorize(Policy = "AdminOnly")]
public class AuditLogController(IUnitOfWork uow) : ControllerBase
{
    /// <summary>GET /api/admin/audit-log?action=&amp;entityType=&amp;userId=&amp;from=&amp;to=&amp;page=&amp;pageSize=</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? action, [FromQuery] string? entityType, [FromQuery] int? userId,
        [FromQuery] DateTime? from, [FromQuery] DateTime? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken ct = default)
    {
        IQueryable<AuditLog> q = uow.Repository<AuditLog>().Query().Include(a => a.User);
        if (!string.IsNullOrEmpty(action))     q = q.Where(a => a.Action == action);
        if (!string.IsNullOrEmpty(entityType)) q = q.Where(a => a.EntityType == entityType);
        if (userId.HasValue)                   q = q.Where(a => a.UserId == userId);
        if (from.HasValue)                     q = q.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)                        q = q.Where(a => a.Timestamp <= to.Value);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.Id, a.Action, a.EntityType, a.EntityId,
                a.UserId, UserName = a.User != null ? (a.User.Name ?? a.User.Username) : null,
                a.IpAddress, a.Timestamp, a.OldValues, a.NewValues,
            })
            .ToListAsync(ct);

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>GET /api/admin/audit-log/filters — distinct action/entityType values for filter dropdowns</summary>
    [HttpGet("filters")]
    public async Task<IActionResult> Filters(CancellationToken ct)
    {
        var actions = await uow.Repository<AuditLog>().Query()
            .Select(a => a.Action).Distinct().OrderBy(a => a).ToListAsync(ct);
        var entityTypes = await uow.Repository<AuditLog>().Query()
            .Where(a => a.EntityType != null).Select(a => a.EntityType!).Distinct().OrderBy(e => e).ToListAsync(ct);
        return Ok(new { actions, entityTypes });
    }
}
