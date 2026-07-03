using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class Notification : AuditableEntity
{
    public string   Title       { get; set; } = string.Empty;
    public string?  Body        { get; set; }
    public string   Type        { get; set; } = "info";
    public string?  Icon        { get; set; }
    public string?  TargetRole  { get; set; }   // null = all
    public string?  TargetGrade { get; set; }
    public int?     CreatedById { get; set; }
    public DateTime? ExpiresAt  { get; set; }

    public User?  CreatedBy         { get; set; }
    public Grade? Grade             { get; set; }
    public ICollection<UserNotification> UserNotifications { get; set; } = [];
}

public class UserNotification
{
    public long     Id             { get; set; }
    public int      UserId         { get; set; }
    public int      NotificationId { get; set; }
    public bool     IsRead         { get; set; }
    public DateTime? ReadAt        { get; set; }
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;

    public User         User         { get; set; } = null!;
    public Notification Notification { get; set; } = null!;
}

public class NotificationTemplate
{
    public int      Id        { get; set; }
    public string   Title     { get; set; } = string.Empty;
    public string?  Body      { get; set; }
    public string   Type      { get; set; } = "info";
    public string?  Icon      { get; set; }
    public bool     IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
