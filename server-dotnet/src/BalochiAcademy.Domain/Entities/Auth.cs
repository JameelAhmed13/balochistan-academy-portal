using BalochiAcademy.Domain.Common;
using BalochiAcademy.Domain.Enums;

namespace BalochiAcademy.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public int      UserId          { get; set; }
    public string   Token           { get; set; } = string.Empty;
    public DateTime ExpiresAt       { get; set; }
    public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt      { get; set; }
    public string?  ReplacedByToken { get; set; }

    public bool IsActive  => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public User User { get; set; } = null!;
}

public class LoginHistory
{
    public long     Id         { get; set; }
    public int?     UserId     { get; set; }
    public DateTime Timestamp  { get; set; } = DateTime.UtcNow;
    public string?  Status     { get; set; }   // 'ok' | 'logout' | 'failed'
    public string?  IpAddress  { get; set; }
    public string?  UserAgent  { get; set; }
    public string?  DeviceInfo { get; set; }

    public User? User { get; set; }
}
