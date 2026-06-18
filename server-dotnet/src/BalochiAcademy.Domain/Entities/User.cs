using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class User : AuditableEntity
{
    public string Username     { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int    RoleId       { get; set; }
    public string? Name        { get; set; }
    public string? Phone       { get; set; }
    public string? Email       { get; set; }
    public string? GradeCode   { get; set; }
    public string  Medium      { get; set; } = "English";
    public string? Institute   { get; set; }
    public string  Board       { get; set; } = "Balochistan";
    public int     Coins       { get; set; }
    public bool    IsActive    { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public Role            Role             { get; set; } = null!;
    public Grade?          Grade            { get; set; }
    public ICollection<RefreshToken>       RefreshTokens    { get; set; } = [];
    public ICollection<LoginHistory>       LoginHistories   { get; set; } = [];
    public ICollection<TestAttempt>        TestAttempts     { get; set; } = [];
    public ICollection<CoinLedger>         CoinLedger       { get; set; } = [];
    public ICollection<Complaint>          Complaints       { get; set; } = [];
    public ICollection<UserNotification>   Notifications    { get; set; } = [];
    public ICollection<StudentNote>        Notes            { get; set; } = [];
    public ICollection<StudentProgress>    Progress         { get; set; } = [];
    public PayoutAccount?  PayoutAccount   { get; set; }
}
