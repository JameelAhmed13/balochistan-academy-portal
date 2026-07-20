using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class Test : AuditableEntity
{
    public string   Title       { get; set; } = string.Empty;
    public string?  GradeCode   { get; set; }
    public int?     SubjectId   { get; set; }
    public int?     UnitId      { get; set; }
    public string   Kind        { get; set; } = "objective";
    public int      DurationMin { get; set; } = 30;
    public int?     TotalMarks  { get; set; }
    public bool     IsPublished { get; set; }
    public bool     IsScheduled { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? EndsAt      { get; set; }
    public int?     CreatedById { get; set; }

    public Grade?   Grade     { get; set; }
    public Subject? Subject   { get; set; }
    public Unit?    Unit      { get; set; }
    public User?    CreatedBy { get; set; }
    public ICollection<TestQuestion> TestQuestions { get; set; } = [];
    public ICollection<TestAttempt>  Attempts      { get; set; } = [];
}

public class TestQuestion
{
    public int TestId     { get; set; }
    public int QuestionId { get; set; }
    public int SortOrder  { get; set; }

    public Test     Test     { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

public class TestAttempt : AuditableEntity
{
    public int     UserId      { get; set; }
    public int?    TestId      { get; set; }
    public string? GradeCode   { get; set; }
    public int?    SubjectId   { get; set; }
    public int?    Score       { get; set; }
    public int?    Total       { get; set; }
    public decimal? Percent    { get; set; }
    public int?    DurationSec { get; set; }
    public string? AnswersJson { get; set; }   // JSON: [{questionId,chosen,correct,marks,timeSpent}]
    public string? AttemptType { get; set; }
    public int     CoinsEarned { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public User    User    { get; set; } = null!;
    public Test?   Test    { get; set; }
    public ICollection<CoinLedger> CoinLedgers { get; set; } = [];
}
