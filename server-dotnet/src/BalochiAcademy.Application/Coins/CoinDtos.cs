namespace BalochiAcademy.Application.Coins;

public record CoinLedgerEntryDto(
    long     Id,
    int      Amount,
    string?  Reason,
    DateTime Timestamp
);
