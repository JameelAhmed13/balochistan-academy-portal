using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Complaints;

public record ComplaintDto(
    int      Id,
    int?     UserId,
    string?  UserName,
    string   Category,
    string   Subject,
    string   Description,
    string   Status,
    int      MessageCount,
    DateTime CreatedAt,
    DateTime? ResolvedAt
);

public record ComplaintMessageDto(
    int      Id,
    int      SenderId,
    string?  SenderName,
    bool     IsAdmin,
    string   Message,
    DateTime CreatedAt
);

public record CreateComplaintRequest(
    [Required] string Subject,
    [Required] string Description,
    string Category = "Complaint"
);

public record SendComplaintMessageRequest(
    [Required] string Message
);

public record UpdateComplaintStatusRequest(
    [Required] string Status
);
