using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/admin/subscriptions")]
[Authorize(Policy = "AdminOnly")]
public class SubscriptionController(IUnitOfWork uow, ICurrentUserService cu, IAuditService audit) : ControllerBase
{
    /// <summary>GET /api/admin/subscriptions — list all plans</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await uow.Repository<SubscriptionPlan>().Query()
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Description, p.Price, p.DurationDays, p.AiTokenQuota, p.IsActive, p.SortOrder })
            .ToListAsync(ct));

    /// <summary>POST /api/admin/subscriptions — create plan</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubscriptionPlanRequest req, CancellationToken ct)
    {
        var plan = new SubscriptionPlan
        {
            Name = req.Name, Description = req.Description,
            Price = req.Price, DurationDays = req.DurationDays, AiTokenQuota = req.AiTokenQuota,
            IsActive = req.IsActive, SortOrder = req.SortOrder,
        };
        uow.Repository<SubscriptionPlan>().Add(plan);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Create", "SubscriptionPlan", plan.Id,
            newValues: new { plan.Name, plan.Price }, ip: cu.IpAddress, ct: ct);
        return Created($"/api/admin/subscriptions/{plan.Id}",
            new { plan.Id, plan.Name, plan.Description, plan.Price, plan.DurationDays, plan.AiTokenQuota, plan.IsActive, plan.SortOrder });
    }

    /// <summary>PUT /api/admin/subscriptions/{id} — update plan</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SubscriptionPlanRequest req, CancellationToken ct)
    {
        var plan = await uow.Repository<SubscriptionPlan>().FindAsync([id], ct);
        if (plan == null) return NotFound();
        plan.Name = req.Name; plan.Description = req.Description;
        plan.Price = req.Price; plan.DurationDays = req.DurationDays; plan.AiTokenQuota = req.AiTokenQuota;
        plan.IsActive = req.IsActive; plan.SortOrder = req.SortOrder;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Update", "SubscriptionPlan", id,
            newValues: new { plan.Name, plan.Price }, ip: cu.IpAddress, ct: ct);
        return Ok(new { plan.Id, plan.Name, plan.Description, plan.Price, plan.DurationDays, plan.AiTokenQuota, plan.IsActive, plan.SortOrder });
    }

    /// <summary>DELETE /api/admin/subscriptions/{id} — soft-delete (IsActive = false)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var plan = await uow.Repository<SubscriptionPlan>().FindAsync([id], ct);
        if (plan == null) return NotFound();
        plan.IsActive = false;
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Delete", "SubscriptionPlan", id, oldValues: new { plan.Name }, ip: cu.IpAddress, ct: ct);
        return Ok(new { ok = true });
    }

    /// <summary>GET /api/admin/subscriptions/users/{userId} — current subscription for a user</summary>
    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> GetUserSubscription(int userId, CancellationToken ct)
    {
        var sub = await uow.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && s.Status == "active")
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);
        if (sub == null) return Ok((object?)null);
        return Ok(new
        {
            sub.Id, sub.UserId, sub.PlanId, sub.StartDate, sub.EndDate, sub.Status,
            sub.AiTokenQuota, sub.AiTokensUsedThisPeriod,
            PlanName = sub.Plan.Name, sub.Plan.Price, sub.Plan.DurationDays,
        });
    }

    /// <summary>POST /api/admin/subscriptions/users/{userId} — assign plan to user</summary>
    [HttpPost("users/{userId:int}")]
    public async Task<IActionResult> AssignPlan(int userId, [FromBody] AssignPlanRequest req, CancellationToken ct)
    {
        var plan = await uow.Repository<SubscriptionPlan>().FindAsync([req.PlanId], ct);
        if (plan == null) return BadRequest(new { error = "Plan not found" });

        var existing = await uow.Repository<UserSubscription>().Query()
            .Where(s => s.UserId == userId && s.Status == "active")
            .ToListAsync(ct);
        foreach (var s in existing) s.Status = "expired";

        var start = DateTime.UtcNow;
        var sub = new UserSubscription
        {
            UserId = userId, PlanId = req.PlanId,
            StartDate = start, EndDate = start.AddDays(plan.DurationDays),
            Status = "active", AiTokenQuota = plan.AiTokenQuota, AiTokensUsedThisPeriod = 0,
        };
        uow.Repository<UserSubscription>().Add(sub);
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, "Assign", "Subscription", sub.Id,
            newValues: new { userId, plan.Name }, ip: cu.IpAddress, ct: ct);
        return Created($"/api/admin/subscriptions/users/{userId}",
            new { sub.Id, sub.UserId, sub.PlanId, sub.StartDate, sub.EndDate, sub.Status, sub.AiTokenQuota });
    }

    /// <summary>GET /api/admin/subscriptions/pending — cash purchases awaiting verification</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> ListPending(CancellationToken ct)
        => Ok(await uow.Repository<UserSubscription>().Query()
            .Include(s => s.Plan).Include(s => s.User)
            .Where(s => s.Status == "pending_payment")
            .OrderBy(s => s.Id)
            .Select(s => new
            {
                s.Id, s.UserId, UserName = s.User.Name ?? s.User.Username,
                s.PlanId, PlanName = s.Plan.Name, s.Plan.Price,
                s.PaymentMethod, s.ReferenceNumber, SubmittedAt = s.StartDate,
            })
            .ToListAsync(ct));

    /// <summary>PUT /api/admin/subscriptions/pending/{id} — approve (activate) or reject a cash purchase</summary>
    [HttpPut("pending/{id:int}")]
    public async Task<IActionResult> ResolvePending(int id, [FromBody] ResolvePendingRequest req, CancellationToken ct)
    {
        var sub = await uow.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id && s.Status == "pending_payment", ct);
        if (sub == null) return NotFound();

        if (req.Approve)
        {
            var existing = await uow.Repository<UserSubscription>().Query()
                .Where(s => s.UserId == sub.UserId && s.Status == "active")
                .ToListAsync(ct);
            foreach (var s in existing) s.Status = "expired";

            var start = DateTime.UtcNow;
            sub.StartDate = start;
            sub.EndDate = start.AddDays(sub.Plan.DurationDays);
            sub.Status = "active";
        }
        else
        {
            sub.Status = "rejected";
        }
        await uow.SaveChangesAsync(ct);
        await audit.LogAsync(cu.UserId, req.Approve ? "Approve" : "Reject", "CashSubscription", id,
            newValues: new { sub.UserId, PlanName = sub.Plan.Name, sub.PaymentMethod, sub.ReferenceNumber }, ip: cu.IpAddress, ct: ct);
        return Ok(new { sub.Id, sub.Status });
    }
}

public record ResolvePendingRequest(bool Approve);

public record SubscriptionPlanRequest(
    string  Name,
    string? Description  = null,
    decimal Price        = 0,
    int     DurationDays = 30,
    int     AiTokenQuota = 0,
    bool    IsActive     = true,
    int     SortOrder    = 0);

public record AssignPlanRequest(int PlanId);
