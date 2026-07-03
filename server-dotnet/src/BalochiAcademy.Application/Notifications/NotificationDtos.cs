using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Notifications;

public record NotificationDto(
    int      Id,
    string   Title,
    string?  Body,
    string   Type,
    string?  Icon,
    bool     IsRead,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);

public record SendNotificationRequest(
    [Required] string Title,
    string? Body        = null,
    string  Type        = "info",
    string? Icon        = null,
    string? TargetRole  = null,
    string? TargetGrade = null,
    DateTime? ExpiresAt = null
);

public record NotificationTemplateRequest(
    [Required] string Title,
    string? Body = null,
    string? Type = "info",
    string? Icon = null
);
