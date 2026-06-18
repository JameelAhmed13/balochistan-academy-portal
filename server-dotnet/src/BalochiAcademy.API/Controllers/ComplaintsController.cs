using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Application.Complaints;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController(IApplicationDbContext db, ICurrentUserService cu) : ControllerBase
{
    /// <summary>POST /api/complaints — student submits a complaint</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateComplaintRequest req, CancellationToken ct)
    {
        var complaint = new Complaint
        {
            UserId = cu.UserId, Category = req.Category,
            Subject = req.Subject, Description = req.Description,
        };
        db.Complaints.Add(complaint);
        await db.SaveChangesAsync(ct);
        return Created($"/api/complaints/{complaint.Id}", complaint);
    }

    /// <summary>GET /api/complaints — student's own complaints</summary>
    [HttpGet]
    public async Task<IActionResult> MyComplaints(CancellationToken ct)
    {
        var items = await db.Complaints
            .Where(c => c.UserId == cu.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ComplaintDto(c.Id, null, c.Category, c.Subject, c.Description, c.Status, c.AdminReply, c.CreatedAt, c.ResolvedAt))
            .ToListAsync(ct);
        return Ok(items);
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/complaints</summary>
    [HttpGet("/api/admin/complaints"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminList([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var q = db.Complaints.Include(c => c.User).AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(c => c.Status == status);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new ComplaintDto(c.Id, c.User != null ? c.User.Name : null, c.Category, c.Subject, c.Description, c.Status, c.AdminReply, c.CreatedAt, c.ResolvedAt))
            .ToListAsync(ct);
        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>PUT /api/admin/complaints/{id} — admin replies</summary>
    [HttpPut("/api/admin/complaints/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyComplaintRequest req, CancellationToken ct)
    {
        var c = await db.Complaints.FindAsync(id, ct);
        if (c == null) return NotFound();
        c.AdminReply  = req.AdminReply;
        c.Status      = req.Status;
        c.HandledById = cu.UserId;
        if (req.Status is "resolved" or "closed") c.ResolvedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(c);
    }
}
