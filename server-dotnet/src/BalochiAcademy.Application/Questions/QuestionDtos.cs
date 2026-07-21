using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Questions;

public record QuestionDto(
    int     Id,
    string? GradeCode,
    int?    SubjectId,
    int?    UnitId,
    int?    TopicId,
    string  Kind,
    string  Stem,
    string? OptionsJson,
    int?    CorrectIndex,
    string? QuestionType,
    int?    Marks,
    string? ModelAnswer,
    string? Difficulty,
    string? CognitiveLevel,
    string? SloCode,
    bool    IsEntranceExam,
    bool    IsAiGenerated,
    string? Feedback,
    int?    PastPaperId
);

public record CreateQuestionRequest(
    [Required] string Kind,
    [Required] string Stem,
    string? GradeCode    = null,
    int?    SubjectId    = null,
    int?    UnitId       = null,
    int?    TopicId      = null,
    string? OptionsJson  = null,
    int?    CorrectIndex = null,
    string? QuestionType = null,
    int?    Marks        = null,
    string? ModelAnswer  = null,
    string? Difficulty   = null,
    string? CognitiveLevel = null,
    string? Feedback     = null,
    string? SloCode      = null,
    bool    IsEntranceExam = false,
    bool    IsAiGenerated  = false,
    int?    PastPaperId    = null
);

public record UpdateQuestionRequest(
    string? Stem          = null,
    string? OptionsJson   = null,
    int?    CorrectIndex  = null,
    string? ModelAnswer   = null,
    string? Difficulty    = null,
    string? CognitiveLevel = null,
    string? Feedback      = null,
    int?    Marks         = null,
    int?    PastPaperId   = null,
    bool    ClearPastPaper = false
);

public record QuestionFilterQuery(
    string? GradeCode    = null,
    int?    SubjectId    = null,
    int?    UnitId       = null,
    string? Kind         = null,
    string? Difficulty   = null,
    bool?   IsEntranceExam = null,
    string? Search       = null,
    int     Page         = 1,
    int     PageSize     = 30
);
