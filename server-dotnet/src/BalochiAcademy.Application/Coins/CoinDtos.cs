using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Coins;

public record CoinLedgerEntryDto(
    long     Id,
    int      Amount,
    string?  Reason,
    DateTime Timestamp
);

public record PayoutAccountDto(
    int    Id,
    string AccountName,
    string AccountNo,
    string Service
);

public record UpsertPayoutAccountRequest(
    [Required] string AccountName,
    [Required] string AccountNo,
    [Required] string Service
);

public record WithdrawalRequestDto(
    int      Id,
    int      AmountCoins,
    decimal  AmountPkr,
    string   Status,
    string?  AdminNote,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record CreateWithdrawalRequest([Required, Range(300, int.MaxValue)] int AmountCoins);

public record ProcessWithdrawalRequest(
    [Required] string Status,   // 'approved' | 'rejected' | 'paid'
    string? AdminNote = null
);
