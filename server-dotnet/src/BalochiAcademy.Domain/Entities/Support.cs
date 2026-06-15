using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class Complaint : AuditableEntity
{
    public int?    UserId      { get; set; }
    public string  Category    { get; set; } = "general";
    public string  Subject     { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public string  Status      { get; set; } = "open";
    public string? AdminReply  { get; set; }
    public int?    HandledById { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public User? User      { get; set; }
    public User? HandledBy { get; set; }
}

public class ContentItem : AuditableEntity
{
    public string?  GradeCode    { get; set; }
    public int?     SubjectId    { get; set; }
    public int?     UnitId       { get; set; }
    public int?     TopicId      { get; set; }
    public string   Kind         { get; set; } = string.Empty;
    public string   Title        { get; set; } = string.Empty;
    public string?  Description  { get; set; }
    public string?  Url          { get; set; }
    public string?  ThumbnailUrl { get; set; }
    public string?  FilePath     { get; set; }
    public int?     DurationSec  { get; set; }
    public int      SortOrder    { get; set; }
    public bool     IsPublished  { get; set; }
    public string?  Tags         { get; set; }
    public string?  SourceYear   { get; set; }
    public int?     CreatedById  { get; set; }

    public Grade?   Grade     { get; set; }
    public Subject? Subject   { get; set; }
    public Unit?    Unit      { get; set; }
    public Topic?   Topic     { get; set; }
    public User?    CreatedBy { get; set; }
}

public class VideoCourse : AuditableEntity
{
    public string?  GradeCode    { get; set; }
    public int?     SubjectId    { get; set; }
    public string   Title        { get; set; } = string.Empty;
    public string?  Description  { get; set; }
    public string?  ThumbnailUrl { get; set; }
    public string?  TutorName    { get; set; }
    public bool     IsPublished  { get; set; }
    public int      SortOrder    { get; set; }

    public Grade?   Grade   { get; set; }
    public Subject? Subject { get; set; }
    public ICollection<VideoLesson> Lessons { get; set; } = [];
}

public class VideoLesson : AuditableEntity
{
    public int     CourseId    { get; set; }
    public int?    UnitId      { get; set; }
    public string  Title       { get; set; } = string.Empty;
    public string  VideoUrl    { get; set; } = string.Empty;
    public int?    DurationSec { get; set; }
    public int     SortOrder   { get; set; }
    public string? Description { get; set; }
    public string? Transcript  { get; set; }

    public VideoCourse Course { get; set; } = null!;
    public Unit?       Unit   { get; set; }
}

public class StudentNote : AuditableEntity
{
    public int     UserId    { get; set; }
    public string? GradeCode { get; set; }
    public int?    SubjectId { get; set; }
    public int?    UnitId    { get; set; }
    public string? Title     { get; set; }
    public string? Content   { get; set; }
    public string? Tags      { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User     User    { get; set; } = null!;
    public Subject? Subject { get; set; }
}

public class StudentProgress : AuditableEntity
{
    public int     UserId         { get; set; }
    public string? GradeCode      { get; set; }
    public int?    SubjectId      { get; set; }
    public int     TotalAttempts  { get; set; }
    public int     TotalCorrect   { get; set; }
    public int     TotalQuestions { get; set; }
    public decimal AvgPercent     { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;

    public User     User    { get; set; } = null!;
    public Subject? Subject { get; set; }
}

public class AuditLog
{
    public long     Id         { get; set; }
    public int?     UserId     { get; set; }
    public string   Action     { get; set; } = string.Empty;
    public string?  EntityType { get; set; }
    public int?     EntityId   { get; set; }
    public string?  OldValues  { get; set; }
    public string?  NewValues  { get; set; }
    public string?  IpAddress  { get; set; }
    public DateTime Timestamp  { get; set; } = DateTime.UtcNow;
    public User?    User       { get; set; }
}

public class SystemSetting
{
    public string   Key         { get; set; } = string.Empty;
    public string?  Value       { get; set; }
    public string?  Description { get; set; }
    public DateTime UpdatedAt   { get; set; } = DateTime.UtcNow;
    public int?     UpdatedById { get; set; }
    public User?    UpdatedBy   { get; set; }
}
