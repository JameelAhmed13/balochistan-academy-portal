using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Middleware;

/// <summary>
/// Enforces the 7-day free trial for students: once trial_days have passed since registration
/// and the student has no active subscription, every API call is blocked with 402 except the
/// routes needed to see and act on the paywall (auth, subscriptions, coins, notifications).
/// Admins and teachers are never gated.
/// </summary>
public class TrialGateMiddleware(RequestDelegate next)
{
    private static readonly string[] AllowedPrefixes =
    [
        "/api/auth", "/api/subscriptions", "/api/coins", "/api/notifications",
    ];

    public async Task InvokeAsync(HttpContext ctx, ICurrentUserService cu, IUnitOfWork uow, ISystemSettingsService settings)
    {
        var path = ctx.Request.Path.Value ?? "";
        var gated = path.StartsWith("/api", StringComparison.OrdinalIgnoreCase)
                 && !AllowedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                 && cu.Role == "student" && cu.UserId != null;

        if (gated)
        {
            var userId = cu.UserId!.Value;
            try
            {
                var ct = ctx.RequestAborted;
                var hasActiveSub = await uow.Repository<UserSubscription>().Query()
                    .AnyAsync(s => s.UserId == userId && s.Status == "active" && s.EndDate > DateTime.UtcNow, ct);

                if (!hasActiveSub)
                {
                    var user = await uow.Repository<User>().FindAsync([userId], ct);
                    var trialDays = int.TryParse(await settings.GetAsync("trial_days", "7", ct), out var td) ? td : 7;
                    if (user != null && DateTime.UtcNow > user.CreatedAt.AddDays(trialDays))
                    {
                        ctx.Response.StatusCode = 402;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            error = "Your 7-day free trial has ended. Subscribe to keep learning.",
                            subscriptionRequired = true,
                        }, ct);
                        return;
                    }
                }
            }
            catch
            {
                // Fail open — a bug in the trial gate should never take down the whole API.
            }
        }

        await next(ctx);
    }
}
