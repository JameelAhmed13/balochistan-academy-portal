using BalochiAcademy.Domain.Common;

namespace BalochiAcademy.Domain.Entities;

public class Grade
{
    public string Code      { get; set; } = string.Empty;
    public string Label     { get; set; } = string.Empty;
    public string Band      { get; set; } = string.Empty;
    public int    SortOrder { get; set; }
    public bool   IsEnabled { get; set; } = true;

    public ICollection<GradeSubject>   GradeSubjects { get; set; } = [];
    public ICollection<Unit>           Units         { get; set; } = [];
    public ICollection<User>           Users         { get; set; } = [];
    public ICollection<AiTutor>        AiTutors      { get; set; } = [];
}

public class Subject : AuditableEntity
{
    public string  Name   { get; set; } = string.Empty;
    public string? NameUr { get; set; }
    public string? Icon   { get; set; }
    public string? Color  { get; set; }
    public string  Medium { get; set; } = "english";

    public ICollection<GradeSubject> GradeSubjects { get; set; } = [];
    public ICollection<Unit>         Units         { get; set; } = [];
    public ICollection<AiTutor>      AiTutors      { get; set; } = [];
    public ICollection<Question>     Questions     { get; set; } = [];
}

public class GradeSubject
{
    public string GradeCode { get; set; } = string.Empty;
    public int    SubjectId { get; set; }
    public Grade   Grade   { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}

public class Unit : AuditableEntity
{
    public string  GradeCode   { get; set; } = string.Empty;
    public int     SubjectId   { get; set; }
    public string  Name        { get; set; } = string.Empty;
    public int?    Number      { get; set; }
    public int     SortOrder   { get; set; }
    public string? Description { get; set; }
    public string? Source      { get; set; }

    public Grade             Grade      { get; set; } = null!;
    public Subject           Subject    { get; set; } = null!;
    public ICollection<Topic>              Topics     { get; set; } = [];
    public ICollection<LearningObjective>  Objectives { get; set; } = [];
}

public class Topic : AuditableEntity
{
    public int    UnitId    { get; set; }
    public string Name      { get; set; } = string.Empty;
    public int    SortOrder { get; set; }

    public Unit                           Unit       { get; set; } = null!;
    public ICollection<LearningObjective> Objectives { get; set; } = [];
}

public class LearningObjective : AuditableEntity
{
    public int?    UnitId         { get; set; }
    public int?    TopicId        { get; set; }
    public string? Code           { get; set; }
    public string  ObjectiveText  { get; set; } = string.Empty;
    public string? CognitiveLevel { get; set; }
    public string? Source         { get; set; }
    public bool    IsAiStructured { get; set; }

    public Unit?  Unit  { get; set; }
    public Topic? Topic { get; set; }
}

public class AiTutor : AuditableEntity
{
    public int?    SubjectId    { get; set; }
    public string? GradeCode    { get; set; }
    public string  Persona      { get; set; } = string.Empty;
    public string  Slug         { get; set; } = string.Empty;
    public string? Icon         { get; set; }
    public string? Color        { get; set; }
    public string? Description  { get; set; }
    public string? SystemPrompt { get; set; }
    public bool    IsActive     { get; set; } = true;

    public Subject? Subject { get; set; }
    public Grade?   Grade   { get; set; }
}
