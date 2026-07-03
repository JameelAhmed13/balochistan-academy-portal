using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/admin/subscriptions")]
[Authorize(Policy = "AdminOnly")]
public class SubscriptionController(IUnitOfWork uow) : ControllerBase
{
    /// <summary>GET /api/admin/subscriptions — list all plans</summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await uow.Repository<SubscriptionPlan>().Query()
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Description, p.Price, p.DurationDays, p.IsActive, p.SortOrder })
            .ToListAsync(ct));

    /// <summary>POST /api/admin/subscriptions — create plan</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SubscriptionPlanRequest req, CancellationToken ct)
    {
        var plan = new SubscriptionPlan
        {
            Name = req.Name, Description = req.Description,
            Price = req.Price, DurationDays = req.DurationDays,
            IsActive = req.IsActive, SortOrder = req.SortOrder,
        };
        uow.Repository<SubscriptionPlan>().Add(plan);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/admin/subscriptions/{plan.Id}",
            new { plan.Id, plan.Name, plan.Description, plan.Price, plan.DurationDays, plan.IsActive, plan.SortOrder });
    }

    /// <summary>PUT /api/admin/subscriptions/{id} — update plan</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SubscriptionPlanRequest req, CancellationToken ct)
    {
        var plan = await uow.Repository<SubscriptionPlan>().FindAsync([id], ct);
        if (plan == null) return NotFound();
        plan.Name = req.Name; plan.Description = req.Description;
        plan.Price = req.Price; plan.DurationDays = req.DurationDays;
        plan.IsActive = req.IsActive; plan.SortOrder = req.SortOrder;
        await uow.SaveChangesAsync(ct);
        return Ok(new { plan.Id, plan.Name, plan.Description, plan.Price, plan.DurationDays, plan.IsActive, plan.SortOrder });
    }

    /// <summary>DELETE /api/admin/subscriptions/{id} — soft-delete (IsActive = false)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var plan = await uow.Repository<SubscriptionPlan>().FindAsync([id], ct);
        if (plan == null) return NotFound();
        plan.IsActive = false;
        await uow.SaveChangesAsync(ct);
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
            Status = "active",
        };
        uow.Repository<UserSubscription>().Add(sub);
        await uow.SaveChangesAsync(ct);
        return Created($"/api/admin/subscriptions/users/{userId}",
            new { sub.Id, sub.UserId, sub.PlanId, sub.StartDate, sub.EndDate, sub.Status });
    }
}

public record SubscriptionPlanRequest(
    string  Name,
    string? Description  = null,
    decimal Price        = 0,
    int     DurationDays = 30,
    bool    IsActive     = true,
    int     SortOrder    = 0);

public record AssignPlanRequest(int PlanId);
