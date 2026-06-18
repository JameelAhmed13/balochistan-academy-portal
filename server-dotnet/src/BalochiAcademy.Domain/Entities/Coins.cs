using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class CoinLedger
{
    public long     Id        { get; set; }
    public int      UserId    { get; set; }
    public int      Amount    { get; set; }   // positive = earn, negative = spend
    public string?  Reason    { get; set; }
    public int?     AttemptId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public User        User    { get; set; } = null!;
    public TestAttempt? Attempt { get; set; }
}

public class PayoutAccount : AuditableEntity
{
    public int    UserId      { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNo   { get; set; } = string.Empty;
    public string Service     { get; set; } = string.Empty;   // 'JazzCash','EasyPaisa','BankTransfer'
    public bool   IsDefault   { get; set; } = true;

    public User User { get; set; } = null!;
    public ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = [];
}

public class WithdrawalRequest : AuditableEntity
{
    public int      UserId        { get; set; }
    public int      AmountCoins   { get; set; }
    public decimal  AmountPkr     { get; set; }
    public int?     AccountId     { get; set; }
    public string   Status        { get; set; } = "pending";   // pending|approved|rejected|paid
    public string?  AdminNote     { get; set; }
    public int?     ProcessedById { get; set; }
    public DateTime? ProcessedAt  { get; set; }

    public User          User        { get; set; } = null!;
    public PayoutAccount? Account    { get; set; }
    public User?          ProcessedBy { get; set; }
}
