using System.Text.Json;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

/// <summary>
/// Single aggregated payload for the admin dashboard landing page (AdminView.vue) — one round
/// trip instead of 7+ separate calls to render the banner, KPI row, module badges, and the
/// recent-activity feed.
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController(IUnitOfWork uow) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var totalUsers     = await uow.Repository<User>().Query().CountAsync(ct);
        var totalStudents  = await uow.Repository<User>().Query()
            .CountAsync(u => u.Role.Name == "student" && u.IsActive, ct);
        var totalQuestions = await uow.Repository<Question>().Query().CountAsync(ct);
        var totalTests     = await uow.Repository<Test>().Query().CountAsync(ct);
        var activeTests    = await uow.Repository<Test>().Query().CountAsync(t => t.IsPublished, ct);
        var totalAttempts  = await uow.Repository<TestAttempt>().Query().CountAsync(ct);
        var totalCoins     = await uow.Repository<User>().Query().SumAsync(u => (long)u.Coins, ct);
        var subjectsCount  = await uow.Repository<Subject>().Query().CountAsync(ct);
        var pendingCoins   = await uow.Repository<UserSubscription>().Query()
            .CountAsync(s => s.Status == "pending_payment", ct);
        var notificationsSent = await uow.Repository<Notification>().Query().CountAsync(ct);
        var settingsCount  = await uow.Repository<SystemSetting>().Query().CountAsync(ct);
        var openComplaints = await uow.Repository<Complaint>().Query().CountAsync(c => c.Status == "open", ct);
        var auditLogCount  = await uow.Repository<AuditLog>().Query().CountAsync(ct);

        var byGrade = await uow.Repository<User>().Query()
            .Where(u => u.Role.Name == "student" && u.IsActive && u.GradeCode != null)
            .GroupBy(u => u.GradeCode)
            .Select(g => new { grade = g.Key, count = g.Count() })
            .ToListAsync(ct);

        var recentActivity = await BuildRecentActivityAsync(ct);

        return Ok(new
        {
            totalUsers, totalStudents, totalQuestions, totalTests, activeTests, totalAttempts, totalCoins, byGrade,
            moduleCounts = new
            {
                subjects = subjectsCount, activeTests, users = totalUsers, pendingCoins,
                notificationsSent, settings = settingsCount, openComplaints, auditLogCount,
            },
            recentActivity,
        });
    }

    private record ActivityEntry(DateTime Timestamp, string Icon, string Text, string Color);

    /// <summary>
    /// AuditLog now covers every privileged admin action (users, tests, subscriptions,
    /// complaints, notifications, roles, institutes, settings, catalog) plus registration —
    /// it's the primary source here. Coin redemptions and new questions are student/content
    /// events rather than admin actions, so they're pulled directly instead of polluting the
    /// audit trail with routine business transactions. Registration is excluded from the audit
    /// merge since the direct student query already renders it with friendlier text.
    /// </summary>
    private async Task<List<object>> BuildRecentActivityAsync(CancellationToken ct)
    {
        var entries = new List<ActivityEntry>();

        var newStudents = await uow.Repository<User>().Query()
            .Where(u => u.Role.Name == "student")
            .OrderByDescending(u => u.CreatedAt).Take(5)
            .Select(u => new { u.Name, u.Username, u.CreatedAt })
            .ToListAsync(ct);
        entries.AddRange(newStudents.Select(u =>
            new ActivityEntry(u.CreatedAt, "👤", $"New student registered: {u.Name ?? u.Username}", "#3b82f6")));

        var newRedemptions = await uow.Repository<CoinRedemption>().Query()
            .Include(r => r.User).Include(r => r.Plan)
            .OrderByDescending(r => r.CreatedAt).Take(5)
            .ToListAsync(ct);
        entries.AddRange(newRedemptions.Select(r =>
        {
            var who = r.User?.Name ?? r.User?.Username ?? "A student";
            var text = r.Type switch
            {
                "subscription_purchase" => $"{who} purchased {r.Plan?.Name ?? "a plan"} with coins",
                "subscription_renewal"  => $"{who} renewed {r.Plan?.Name ?? "their plan"} with coins",
                "token_topup"           => $"{who} bought +{r.TokensGranted} AI tokens with coins",
                _                       => $"{who} redeemed {r.CoinsSpent} coins",
            };
            return new ActivityEntry(r.CreatedAt, "🪙", text, "#f59e0b");
        }));

        var newQuestions = await uow.Repository<Question>().Query()
            .OrderByDescending(q => q.CreatedAt).Take(3)
            .Select(q => new { q.Kind, q.CreatedAt })
            .ToListAsync(ct);
        entries.AddRange(newQuestions.Select(q =>
            new ActivityEntry(q.CreatedAt, "❓", $"New {q.Kind} question added to the bank", "#8b5cf6")));

        var auditEntries = await uow.Repository<AuditLog>().Query()
            .Include(a => a.User)
            .Where(a => !(a.EntityType == "User" && a.Action == "Register"))
            .OrderByDescending(a => a.Timestamp).Take(6)
            .ToListAsync(ct);
        entries.AddRange(auditEntries.Select(a =>
        {
            var actor = a.User?.Name ?? a.User?.Username ?? "System";
            var label = ExtractLabel(a.NewValues) ?? ExtractLabel(a.OldValues);
            var text = label != null
                ? $"{a.Action} {a.EntityType} \"{label}\" by {actor}"
                : $"{a.Action} {a.EntityType} by {actor}";
            return new ActivityEntry(a.Timestamp, "🛡️", text, "#6d54e8");
        }));

        return entries
            .OrderByDescending(e => e.Timestamp)
            .Take(8)
            .Select(e => (object)new { e.Icon, e.Text, e.Color, timestamp = e.Timestamp })
            .ToList();
    }

    /// <summary>Best-effort human label from an audit entry's stored JSON (looks for Title, then Name).</summary>
    private static string? ExtractLabel(string? json)
    {
        if (json == null) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Title", out var t)) return t.GetString();
            if (doc.RootElement.TryGetProperty("Name", out var n)) return n.GetString();
        }
        catch { /* not JSON we can label — fall through */ }
        return null;
    }
}
