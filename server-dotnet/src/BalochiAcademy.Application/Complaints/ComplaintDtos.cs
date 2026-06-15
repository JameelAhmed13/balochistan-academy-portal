using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Complaints;

public record ComplaintDto(
    int      Id,
    string?  UserName,
    string   Category,
    string   Subject,
    string   Description,
    string   Status,
    string?  AdminReply,
    DateTime CreatedAt,
    DateTime? ResolvedAt
);

public record CreateComplaintRequest(
    [Required] string Subject,
    [Required] string Description,
    string Category = "general"
);

public record ReplyComplaintRequest(
    [Required] string AdminReply,
    string Status = "resolved"
);
