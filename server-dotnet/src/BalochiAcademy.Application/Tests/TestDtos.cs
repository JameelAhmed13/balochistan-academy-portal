using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Tests;

public record TestDto(
    int      Id,
    string   Title,
    string?  GradeCode,
    int?     SubjectId,
    int?     UnitId,
    string   Kind,
    int      DurationMin,
    int?     TotalMarks,
    bool     IsPublished,
    int      QuestionCount,
    DateTime CreatedAt
);

public record CreateTestRequest(
    [Required] string Title,
    [Required] string Kind,
    string? GradeCode   = null,
    int?    SubjectId   = null,
    int?    UnitId      = null,
    int     DurationMin = 30,
    int?    TotalMarks  = null,
    int[]?  QuestionIds = null
);

public record SubmitAttemptRequest(
    [Required] int    Total,
    [Required] string AttemptType,
    int?    TestId      = null,
    string? AnswersJson = null,
    int     Score       = 0,
    int     DurationSec = 0
);

public record AttemptResultDto(
    int     Id,
    int     UserId,
    int?    TestId,
    int?    Score,
    int?    Total,
    decimal? Percent,
    int     CoinsEarned,
    string  AttemptType,
    DateTime SubmittedAt
);

public record LeaderboardEntryDto(
    int     UserId,
    string? Name,
    string? GradeCode,
    int     Coins,
    int     TotalAttempts,
    decimal AvgPercent,
    long    CoinRank,
    long    ScoreRank
);
