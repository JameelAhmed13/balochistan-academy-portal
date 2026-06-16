using AutoMapper;
using BalochiAcademy.Application.Complaints;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController(IUnitOfWork uow, ICurrentUserService cu, IMapper mapper) : ControllerBase
{
    /// <summary>POST /api/complaints — student submits a complaint</summary>
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
        return Created($"/api/complaints/{complaint.Id}", mapper.Map<ComplaintDto>(complaint));
    }

    /// <summary>GET /api/complaints — student's own complaints</summary>
    [HttpGet]
    public async Task<IActionResult> MyComplaints(CancellationToken ct)
    {
        var items = await uow.Repository<Complaint>().Query()
            .Where(c => c.UserId == cu.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
        return Ok(mapper.Map<List<ComplaintDto>>(items));
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/complaints</summary>
    [HttpGet("/api/admin/complaints"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminList(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        IQueryable<Complaint> q = uow.Repository<Complaint>().Query().Include(c => c.User);
        if (!string.IsNullOrEmpty(status)) q = q.Where(c => c.Status == status);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        return Ok(new { items = mapper.Map<List<ComplaintDto>>(items), total, page, pageSize });
    }

    /// <summary>PUT /api/admin/complaints/{id} — admin replies</summary>
    [HttpPut("/api/admin/complaints/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyComplaintRequest req, CancellationToken ct)
    {
        var c = await uow.Repository<Complaint>().FindAsync([id], ct);
        if (c == null) return NotFound();
        c.AdminReply  = req.AdminReply;
        c.Status      = req.Status;
        c.HandledById = cu.UserId;
        if (req.Status is "resolved" or "closed") c.ResolvedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        return Ok(mapper.Map<ComplaintDto>(c));
    }
}
