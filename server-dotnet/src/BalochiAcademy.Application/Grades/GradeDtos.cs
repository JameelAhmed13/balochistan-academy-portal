using System.ComponentModel.DataAnnotations;

namespace BalochiAcademy.Application.Grades;

public record GradeDto(
    string Code,
    string Label,
    string Band,
    int    SortOrder,
    bool   IsEnabled,
    int    SubjectCount
);

public record CreateGradeRequest(
    [Required] string Code,
    [Required] string Label,
    [Required] string Band,
    int  SortOrder = 0,
    bool IsEnabled = true
);

public record UpdateGradeRequest(
    string? Label     = null,
    string? Band      = null,
    int?    SortOrder = null,
    bool?   IsEnabled = null
);

public record SetGradeSubjectsRequest([Required] int[] SubjectIds);
