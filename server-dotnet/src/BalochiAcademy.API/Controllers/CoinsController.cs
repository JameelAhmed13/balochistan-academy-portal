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
public class CoinsController(IUnitOfWork uow, ICurrentUserService cu, IMapper mapper) : ControllerBase
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

        return Ok(new { coins = user.Coins, pkr = user.Coins * 0.50m, earnedToday });
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

    /// <summary>GET /api/coins/payout-account</summary>
    [HttpGet("payout-account")]
    public async Task<IActionResult> GetPayoutAccount(CancellationToken ct)
    {
        var acct = await uow.Repository<PayoutAccount>().Query()
            .FirstOrDefaultAsync(p => p.UserId == cu.UserId, ct);
        if (acct == null) return Ok((object?)null);
        return Ok(mapper.Map<PayoutAccountDto>(acct));
    }

    /// <summary>PUT /api/coins/payout-account — create or replace</summary>
    [HttpPut("payout-account")]
    public async Task<IActionResult> UpsertPayoutAccount([FromBody] UpsertPayoutAccountRequest req, CancellationToken ct)
    {
        var acct = await uow.Repository<PayoutAccount>().Query()
            .FirstOrDefaultAsync(p => p.UserId == cu.UserId, ct);
        if (acct == null)
        {
            acct = new PayoutAccount { UserId = cu.UserId!.Value };
            uow.Repository<PayoutAccount>().Add(acct);
        }
        acct.AccountName = req.AccountName;
        acct.AccountNo   = req.AccountNo;
        acct.Service     = req.Service;
        await uow.SaveChangesAsync(ct);
        return Ok(mapper.Map<PayoutAccountDto>(acct));
    }

    /// <summary>POST /api/coins/withdraw — student requests withdrawal</summary>
    [HttpPost("withdraw")]
    public async Task<IActionResult> RequestWithdrawal([FromBody] CreateWithdrawalRequest req, CancellationToken ct)
    {
        var user = await uow.Repository<User>().FindAsync([cu.UserId!.Value], ct);
        if (user == null) return NotFound();
        if (user.Coins < 300)          return BadRequest(new { error = "Minimum 300 coins required" });
        if (user.Coins < req.AmountCoins) return BadRequest(new { error = "Insufficient coins" });

        var acct = await uow.Repository<PayoutAccount>().Query()
            .FirstOrDefaultAsync(p => p.UserId == cu.UserId, ct);
        if (acct == null) return BadRequest(new { error = "No payout account configured" });

        var pending = await uow.Repository<WithdrawalRequest>().Query()
            .AnyAsync(w => w.UserId == cu.UserId && w.Status == "pending", ct);
        if (pending) return BadRequest(new { error = "A withdrawal request is already pending" });

        var wr = new WithdrawalRequest
        {
            UserId      = cu.UserId!.Value,
            AmountCoins = req.AmountCoins,
            AmountPkr   = req.AmountCoins * 0.50m,
            AccountId   = acct.Id,
        };
        uow.Repository<WithdrawalRequest>().Add(wr);

        user.Coins -= req.AmountCoins;
        uow.Repository<CoinLedger>().Add(new CoinLedger
        {
            UserId = cu.UserId.Value, Amount = -req.AmountCoins,
            Reason = "Withdrawal Request",
        });
        await uow.SaveChangesAsync(ct);
        return Created($"/api/coins/withdrawals/{wr.Id}", mapper.Map<WithdrawalRequestDto>(wr));
    }

    /// <summary>GET /api/coins/withdrawals</summary>
    [HttpGet("withdrawals")]
    public async Task<IActionResult> MyWithdrawals(CancellationToken ct)
    {
        var items = await uow.Repository<WithdrawalRequest>().Query()
            .Where(w => w.UserId == cu.UserId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
        return Ok(mapper.Map<List<WithdrawalRequestDto>>(items));
    }

    // ── Admin ──────────────────────────────────────────────────────────────────

    /// <summary>GET /api/admin/coins/withdrawals — all withdrawal requests</summary>
    [HttpGet("/api/admin/coins/withdrawals"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminWithdrawals(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        IQueryable<WithdrawalRequest> q = uow.Repository<WithdrawalRequest>().Query().Include(w => w.User);
        if (!string.IsNullOrEmpty(status)) q = q.Where(w => w.Status == status);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(w => new
            {
                w.Id, w.AmountCoins, w.AmountPkr, w.Status, w.AdminNote,
                w.CreatedAt, w.ProcessedAt,
                UserName = w.User != null ? w.User.Name : null,
                w.UserId,
            })
            .ToListAsync(ct);
        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>PUT /api/admin/coins/withdrawals/{id} — process withdrawal</summary>
    [HttpPut("/api/admin/coins/withdrawals/{id:int}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ProcessWithdrawal(int id, [FromBody] ProcessWithdrawalRequest req, CancellationToken ct)
    {
        var wr = await uow.Repository<WithdrawalRequest>().Query()
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
        if (wr == null) return NotFound();
        if (wr.Status != "pending") return BadRequest(new { error = "Only pending requests can be processed" });

        wr.Status        = req.Status;
        wr.AdminNote     = req.AdminNote;
        wr.ProcessedById = cu.UserId;
        wr.ProcessedAt   = DateTime.UtcNow;

        if (req.Status == "rejected" && wr.User != null)
        {
            wr.User.Coins += wr.AmountCoins;
            uow.Repository<CoinLedger>().Add(new CoinLedger
            {
                UserId = wr.UserId, Amount = wr.AmountCoins,
                Reason = "Withdrawal Refund (rejected)",
            });
        }
        await uow.SaveChangesAsync(ct);
        return Ok(new { wr.Id, wr.Status, wr.AdminNote, wr.ProcessedAt });
    }
}
