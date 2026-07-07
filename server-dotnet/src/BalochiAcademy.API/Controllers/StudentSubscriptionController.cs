using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

/// <summary>
/// Student-facing subscription redemption — coins buy a new subscription, a renewal of the
/// current one, or extra AI tokens. Separate from SubscriptionController (admin-only plan CRUD
/// and manual assignment) because these routes need any authenticated user, not AdminOnly.
/// </summary>
[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class StudentSubscriptionController(IUnitOfWork uow, ICurrentUserService cu, ISystemSettingsService settings) : ControllerBase
{
    /// <summary>GET /api/subscriptions/plans — active plans with coin cost</summary>
    [HttpGet("plans")]
    public async Task<IActionResult> Plans(CancellationToken ct)
    {
        var rate = decimal.Parse(await settings.GetAsync("coin_rate_pkr", "0.005", ct) ?? "0.005");
        var plans = await uow.Repository<SubscriptionPlan>().Query()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Description, p.Price, p.DurationDays, p.AiTokenQuota })
            .ToListAsync(ct);

        var result = plans.Select(p => new
        {
            p.Id, p.Name, p.Description, p.Price, p.DurationDays, p.AiTokenQuota,
            CoinCost = (int)Math.Ceiling(p.Price / rate),
        });
        return Ok(result);
    }

    /// <summary>GET /api/subscriptions/me — current subscription + AI token balance</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return NotFound();

        var sub = await uow.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.UserId == cu.UserId)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);

        object? subDto = null;
        var periodicRemaining = 0;
        if (sub != null)
        {
            periodicRemaining = Math.Max(0, sub.AiTokenQuota - sub.AiTokensUsedThisPeriod);
            subDto = new
            {
                sub.Id, sub.PlanId, PlanName = sub.Plan.Name, sub.StartDate, sub.EndDate, sub.Status,
                sub.AiTokenQuota, sub.AiTokensUsedThisPeriod, TokensRemainingThisPeriod = periodicRemaining,
            };
        }

        var pending = await uow.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.UserId == cu.UserId && s.Status == "pending_payment")
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(ct);
        object? pendingDto = pending == null ? null : new
        {
            pending.Id, pending.PlanId, PlanName = pending.Plan.Name,
            pending.PaymentMethod, pending.ReferenceNumber,
        };

        var coinsPerAiToken = decimal.Parse(await settings.GetAsync("coins_per_ai_token", "20", ct) ?? "20");

        var hasActiveSub = sub != null && sub.Status == "active" && sub.EndDate > DateTime.UtcNow;
        var trialDays = int.TryParse(await settings.GetAsync("trial_days", "7", ct), out var td) ? td : 7;
        var trialEndsAt = user.CreatedAt.AddDays(trialDays);
        var onTrial = !hasActiveSub && DateTime.UtcNow < trialEndsAt;
        var accessStatus = hasActiveSub ? "subscribed" : (onTrial ? "trial" : "expired");
        var trialDaysLeft = onTrial ? (int)Math.Ceiling((trialEndsAt - DateTime.UtcNow).TotalDays) : 0;

        return Ok(new
        {
            subscription = subDto,
            pendingCashPurchase = pendingDto,
            bonusAiTokens = user.BonusAiTokens,
            totalAiTokensAvailable = periodicRemaining + user.BonusAiTokens,
            coinsPerAiToken,
            accessStatus, trialEndsAt, trialDaysLeft,
        });
    }

    /// <summary>POST /api/subscriptions/me/purchase-cash — submit a real-money payment for admin verification</summary>
    [HttpPost("me/purchase-cash")]
    public async Task<IActionResult> PurchaseCash([FromBody] PurchaseCashRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.PaymentMethod) || string.IsNullOrWhiteSpace(req.ReferenceNumber))
            return BadRequest(new { error = "Payment method and reference number are required" });

        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return NotFound();

        var plan = await uow.Repository<SubscriptionPlan>().Query()
            .FirstOrDefaultAsync(p => p.Id == req.PlanId && p.IsActive, ct);
        if (plan == null) return BadRequest(new { error = "Plan not found" });

        var alreadyPending = await uow.Repository<UserSubscription>().Query()
            .AnyAsync(s => s.UserId == cu.UserId && s.Status == "pending_payment", ct);
        if (alreadyPending)
            return BadRequest(new { error = "You already have a payment awaiting verification." });

        // StartDate/EndDate are placeholders recalculated by the admin on approval — the plan
        // period only actually starts once the payment is verified.
        var sub = new UserSubscription
        {
            UserId = user.Id, PlanId = plan.Id,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(plan.DurationDays),
            Status = "pending_payment", AiTokenQuota = plan.AiTokenQuota, AiTokensUsedThisPeriod = 0,
            PaymentMethod = req.PaymentMethod, ReferenceNumber = req.ReferenceNumber,
        };
        uow.Repository<UserSubscription>().Add(sub);
        await uow.SaveChangesAsync(ct);

        return Ok(new { sub.Id, sub.PlanId, sub.Status, sub.PaymentMethod, sub.ReferenceNumber });
    }

    /// <summary>POST /api/subscriptions/me/purchase — redeem coins for a new (or switched) subscription</summary>
    [HttpPost("me/purchase")]
    public async Task<IActionResult> Purchase([FromBody] PurchaseSubscriptionRequest req, CancellationToken ct)
    {
        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return NotFound();

        var plan = await uow.Repository<SubscriptionPlan>().Query()
            .FirstOrDefaultAsync(p => p.Id == req.PlanId && p.IsActive, ct);
        if (plan == null) return BadRequest(new { error = "Plan not found" });

        var activeSub = await uow.Repository<UserSubscription>().Query()
            .FirstOrDefaultAsync(s => s.UserId == cu.UserId && s.Status == "active" && s.EndDate > DateTime.UtcNow, ct);
        if (activeSub != null)
            return BadRequest(new { error = "You already have an active subscription — use Renew instead of Buy New." });

        var rate = decimal.Parse(await settings.GetAsync("coin_rate_pkr", "0.005", ct) ?? "0.005");
        var coinCost = (int)Math.Ceiling(plan.Price / rate);
        if (user.Coins < coinCost)
            return BadRequest(new { error = $"Insufficient coins — need {coinCost}, have {user.Coins}" });

        // Clears any stale rows still flagged "active" past their end date (normal expiry hasn't run yet).
        var existing = await uow.Repository<UserSubscription>().Query()
            .Where(s => s.UserId == cu.UserId && s.Status == "active")
            .ToListAsync(ct);
        foreach (var s in existing) s.Status = "expired";

        var start = DateTime.UtcNow;
        var sub = new UserSubscription
        {
            UserId = user.Id, PlanId = plan.Id,
            StartDate = start, EndDate = start.AddDays(plan.DurationDays), Status = "active",
            AiTokenQuota = plan.AiTokenQuota, AiTokensUsedThisPeriod = 0,
        };
        uow.Repository<UserSubscription>().Add(sub);
        await uow.SaveChangesAsync(ct); // assigns sub.Id, referenced by the redemption record below

        user.Coins -= coinCost;
        uow.Repository<CoinLedger>().Add(new CoinLedger
        {
            UserId = user.Id, Amount = -coinCost, Reason = $"Subscription purchase: {plan.Name}",
        });
        uow.Repository<CoinRedemption>().Add(new CoinRedemption
        {
            UserId = user.Id, Type = "subscription_purchase", PlanId = plan.Id, SubscriptionId = sub.Id, CoinsSpent = coinCost,
        });
        await uow.SaveChangesAsync(ct);

        return Ok(new { sub.Id, sub.PlanId, sub.StartDate, sub.EndDate, sub.Status, sub.AiTokenQuota, CoinsSpent = coinCost, RemainingCoins = user.Coins });
    }

    /// <summary>POST /api/subscriptions/me/renew — redeem coins to extend the current plan</summary>
    [HttpPost("me/renew")]
    public async Task<IActionResult> Renew(CancellationToken ct)
    {
        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return NotFound();

        var current = await uow.Repository<UserSubscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.UserId == cu.UserId)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);
        if (current == null) return BadRequest(new { error = "No subscription to renew — buy a new one instead" });

        var plan = current.Plan;
        var rate = decimal.Parse(await settings.GetAsync("coin_rate_pkr", "0.005", ct) ?? "0.005");
        var coinCost = (int)Math.Ceiling(plan.Price / rate);
        if (user.Coins < coinCost)
            return BadRequest(new { error = $"Insufficient coins — need {coinCost}, have {user.Coins}" });

        // Extend from the current end date if still active, otherwise start fresh from today.
        var baseDate = current.Status == "active" && current.EndDate > DateTime.UtcNow ? current.EndDate : DateTime.UtcNow;
        current.Status = "expired";

        var sub = new UserSubscription
        {
            UserId = user.Id, PlanId = plan.Id,
            StartDate = DateTime.UtcNow, EndDate = baseDate.AddDays(plan.DurationDays), Status = "active",
            AiTokenQuota = plan.AiTokenQuota, AiTokensUsedThisPeriod = 0,
        };
        uow.Repository<UserSubscription>().Add(sub);
        await uow.SaveChangesAsync(ct); // assigns sub.Id, referenced by the redemption record below

        user.Coins -= coinCost;
        uow.Repository<CoinLedger>().Add(new CoinLedger
        {
            UserId = user.Id, Amount = -coinCost, Reason = $"Subscription renewal: {plan.Name}",
        });
        uow.Repository<CoinRedemption>().Add(new CoinRedemption
        {
            UserId = user.Id, Type = "subscription_renewal", PlanId = plan.Id, SubscriptionId = sub.Id, CoinsSpent = coinCost,
        });
        await uow.SaveChangesAsync(ct);

        return Ok(new { sub.Id, sub.PlanId, sub.StartDate, sub.EndDate, sub.Status, sub.AiTokenQuota, CoinsSpent = coinCost, RemainingCoins = user.Coins });
    }

    /// <summary>POST /api/subscriptions/me/buy-tokens — redeem coins for extra AI tokens (persist across renewals)</summary>
    [HttpPost("me/buy-tokens")]
    public async Task<IActionResult> BuyTokens([FromBody] BuyTokensRequest req, CancellationToken ct)
    {
        if (req.TokenAmount <= 0) return BadRequest(new { error = "Token amount must be positive" });

        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return NotFound();

        var coinsPerToken = decimal.Parse(await settings.GetAsync("coins_per_ai_token", "20", ct) ?? "20");
        var coinCost = (int)Math.Ceiling(req.TokenAmount * coinsPerToken);
        if (user.Coins < coinCost)
            return BadRequest(new { error = $"Insufficient coins — need {coinCost}, have {user.Coins}" });

        user.Coins -= coinCost;
        user.BonusAiTokens += req.TokenAmount;
        uow.Repository<CoinLedger>().Add(new CoinLedger
        {
            UserId = user.Id, Amount = -coinCost, Reason = $"AI token top-up: +{req.TokenAmount} tokens",
        });
        uow.Repository<CoinRedemption>().Add(new CoinRedemption
        {
            UserId = user.Id, Type = "token_topup", TokensGranted = req.TokenAmount, CoinsSpent = coinCost,
        });
        await uow.SaveChangesAsync(ct);

        return Ok(new { RemainingCoins = user.Coins, user.BonusAiTokens, CoinsSpent = coinCost });
    }
}

public record PurchaseSubscriptionRequest(int PlanId);
public record BuyTokensRequest(int TokenAmount);
public record PurchaseCashRequest(int PlanId, string PaymentMethod, string ReferenceNumber);
