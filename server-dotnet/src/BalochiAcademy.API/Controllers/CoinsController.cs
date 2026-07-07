using AutoMapper;
using BalochiAcademy.Application.Coins;
using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.API.Controllers;

[ApiController]
[Route("api/coins")]
[Authorize]
public class CoinsController(IUnitOfWork uow, ICurrentUserService cu, IMapper mapper, ISystemSettingsService settings) : ControllerBase
{
    /// <summary>GET /api/coins/balance</summary>
    [HttpGet("balance")]
    public async Task<IActionResult> Balance(CancellationToken ct)
    {
        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return NotFound();

        var todayStart = DateTime.UtcNow.Date;
        var todayEnd   = todayStart.AddDays(1);
        var earnedToday = await uow.Repository<CoinLedger>().Query()
            .Where(c => c.UserId == cu.UserId && c.Amount > 0
                     && c.Timestamp >= todayStart && c.Timestamp < todayEnd)
            .SumAsync(c => c.Amount, ct);

        var rate = decimal.Parse(await settings.GetAsync("coin_rate_pkr", "0.005", ct) ?? "0.005");
        return Ok(new { coins = user.Coins, pkr = user.Coins * rate, earnedToday });
    }

    /// <summary>GET /api/coins/transactions</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken ct = default)
    {
        var q = uow.Repository<CoinLedger>().Query().Where(c => c.UserId == cu.UserId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        return Ok(new { items = mapper.Map<List<CoinLedgerEntryDto>>(items), total, page, pageSize });
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/coins/redemptions — recent subscription/token redemptions</summary>
    [HttpGet("/api/admin/coins/redemptions"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminRedemptions(
        [FromQuery] string? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        IQueryable<CoinRedemption> q = uow.Repository<CoinRedemption>().Query()
            .Include(r => r.User).Include(r => r.Plan);
        if (!string.IsNullOrEmpty(type)) q = q.Where(r => r.Type == type);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new
            {
                r.Id, r.Type, r.CoinsSpent, r.TokensGranted, r.CreatedAt,
                UserName = r.User != null ? r.User.Name : null,
                PlanName = r.Plan != null ? r.Plan.Name : null,
            })
            .ToListAsync(ct);
        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>GET /api/admin/coins/top-earners — top N students by total coins earned (lifetime)</summary>
    [HttpGet("/api/admin/coins/top-earners"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> TopEarners([FromQuery] int limit = 5, CancellationToken ct = default)
    {
        var top = await uow.Repository<CoinLedger>().Query()
            .Where(c => c.Amount > 0)
            .GroupBy(c => c.UserId)
            .Select(g => new { UserId = g.Key, TotalCoins = g.Sum(c => c.Amount) })
            .OrderByDescending(g => g.TotalCoins)
            .Take(limit)
            .ToListAsync(ct);

        var userIds = top.Select(t => t.UserId).ToList();
        var names = await uow.Repository<User>().Query()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name ?? u.Username, ct);

        var result = top.Select(t => new { UserName = names.GetValueOrDefault(t.UserId, "Unknown"), t.TotalCoins });
        return Ok(result);
    }
}
