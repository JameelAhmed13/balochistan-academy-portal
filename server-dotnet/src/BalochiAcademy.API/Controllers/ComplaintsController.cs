using AutoMapper;
using BalochiAcademy.API.Hubs;
using BalochiAcademy.Application.Complaints;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController(
    IUnitOfWork uow, ICurrentUserService cu, IMapper mapper, IAuditService audit,
    IHubContext<NotificationHub> hub) : ControllerBase
{
    /// <summary>POST /api/complaints — student submits a complaint (opens a new thread)</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComplaintRequest req, CancellationToken ct)
    {
        var complaint = new Complaint
        {
            UserId      = cu.UserId,
            Category    = req.Category,
            Subject     = req.Subject,
            Description = req.Description,
        };
        uow.Repository<Complaint>().Add(complaint);
        await uow.SaveChangesAsync(ct);

        var dto = mapper.Map<ComplaintDto>(complaint);
        await NotificationHub.SendToAdmins(hub, new { type = "complaintCreated", complaint = dto });

        return Created($"/api/complaints/{complaint.Id}", dto);
    }

    /// <summary>GET /api/complaints — student's own complaints</summary>
    [HttpGet]
    public async Task<IActionResult> MyComplaints(CancellationToken ct)
    {
        var items = await uow.Repository<Complaint>().Query()
            .Include(c => c.Messages)
            .Where(c => c.UserId == cu.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
        return Ok(mapper.Map<List<ComplaintDto>>(items));
    }

    /// <summary>GET /api/complaints/{id}/messages — full conversation thread (owner or admin only)</summary>
    [HttpGet("{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, CancellationToken ct)
    {
        var complaint = await uow.Repository<Complaint>().FindAsync([id], ct);
        if (complaint == null) return NotFound();
        if (!CanAccess(complaint)) return StatusCode(403, new { error = "You do not have access to this complaint." });

        var messages = await uow.Repository<ComplaintMessage>().Query()
            .Include(m => m.Sender)
            .Where(m => m.ComplaintId == id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
        return Ok(mapper.Map<List<ComplaintMessageDto>>(messages));
    }

    /// <summary>POST /api/complaints/{id}/messages — reply in the thread (owner or admin only)</summary>
    [HttpPost("{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendComplaintMessageRequest req, CancellationToken ct)
    {
        var complaint = await uow.Repository<Complaint>().FindAsync([id], ct);
        if (complaint == null) return NotFound();
        if (!CanAccess(complaint)) return StatusCode(403, new { error = "You do not have access to this complaint." });
        if (complaint.Status is "resolved" or "closed")
            return BadRequest(new { error = "This complaint is closed and can no longer receive messages." });

        var isAdmin = cu.Role == "admin";
        var message = new ComplaintMessage
        {
            ComplaintId = id,
            SenderId    = cu.UserId!.Value,
            IsAdmin     = isAdmin,
            Message     = req.Message,
        };
        uow.Repository<ComplaintMessage>().Add(message);

        var statusChangedToInProgress = isAdmin && complaint.Status == "open";
        if (statusChangedToInProgress)
            complaint.Status = "in-progress";

        await uow.SaveChangesAsync(ct);
        if (isAdmin)
            await audit.LogAsync(cu.UserId, "Reply", "Complaint", id, ip: cu.IpAddress, ct: ct);

        message = await uow.Repository<ComplaintMessage>().Query()
            .Include(m => m.Sender).FirstAsync(m => m.Id == message.Id, ct);
        var dto = mapper.Map<ComplaintMessageDto>(message);

        // The sender already has the message from this response — clients de-dupe by message id
        // to ignore their own echo when it comes back via a group broadcast.
        var payload = new { type = "complaintMessage", complaintId = id, isAdmin, message = dto };
        if (isAdmin)
        {
            if (complaint.UserId is { } studentId) await NotificationHub.SendToUser(hub, studentId, payload);
            await NotificationHub.SendToAdmins(hub, payload); // other admins watching the same ticket
        }
        else
        {
            await NotificationHub.SendToAdmins(hub, payload);
        }

        // A student's first admin reply moves the ticket out of "open" automatically — it never goes back.
        if (statusChangedToInProgress)
        {
            var statusPayload = new { type = "complaintStatus", complaintId = id, status = complaint.Status };
            if (complaint.UserId is { } sid) await NotificationHub.SendToUser(hub, sid, statusPayload);
            await NotificationHub.SendToAdmins(hub, statusPayload);
        }

        return Created($"/api/complaints/{id}/messages/{message.Id}", dto);
    }

    private bool CanAccess(Complaint c) => cu.Role == "admin" || c.UserId == cu.UserId;

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/complaints</summary>
    [HttpGet("/api/admin/complaints"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminList(
        [FromQuery] string? status, [FromQuery] string? category,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        IQueryable<Complaint> q = uow.Repository<Complaint>().Query().Include(c => c.User).Include(c => c.Messages);
        if (!string.IsNullOrEmpty(status))   q = q.Where(c => c.Status == status);
        if (!string.IsNullOrEmpty(category)) q = q.Where(c => c.Category == category);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        return Ok(new { items = mapper.Map<List<ComplaintDto>>(items), total, page, pageSize });
    }

    /// <summary>GET /api/admin/complaints/stats — counts per status, for the admin inbox banner</summary>
    [HttpGet("/api/admin/complaints/stats"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Stats(CancellationToken ct)
    {
        var total = await uow.Repository<Complaint>().Query().CountAsync(ct);
        var byStatus = await uow.Repository<Complaint>().Query()
            .GroupBy(c => c.Status)
            .Select(g => new { status = g.Key, count = g.Count() })
            .ToListAsync(ct);
        return Ok(new { total, byStatus });
    }

    /// <summary>PUT /api/admin/complaints/{id} — change ticket status (open/in-progress/resolved/closed)</summary>
    [HttpPut("/api/admin/complaints/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateComplaintStatusRequest req, CancellationToken ct)
    {
        var c = await uow.Repository<Complaint>().Query().Include(x => x.Messages).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return NotFound();
        if (c.Status is "resolved" or "closed")
            return BadRequest(new { error = "This complaint is already closed and its status cannot be changed." });
        if (req.Status == "open" && c.Status != "open")
            return BadRequest(new { error = "A complaint cannot be reverted back to Open status." });
        c.Status      = req.Status;
        c.HandledById = cu.UserId;
        c.ResolvedAt  = req.Status is "resolved" or "closed" ? DateTime.UtcNow : null;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "UpdateStatus", "Complaint", id, newValues: new { c.Status }, ip: cu.IpAddress, ct: ct);

        var payload = new { type = "complaintStatus", complaintId = id, status = c.Status };
        if (c.UserId is { } studentId) await NotificationHub.SendToUser(hub, studentId, payload);
        await NotificationHub.SendToAdmins(hub, payload); // other admins' lists/stats update too

        return Ok(mapper.Map<ComplaintDto>(c));
    }
}
