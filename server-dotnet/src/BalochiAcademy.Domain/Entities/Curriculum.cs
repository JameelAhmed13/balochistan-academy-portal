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
    public ICollection<Book>         Books         { get; set; } = [];
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
    public int?    BookId      { get; set; }   // set when unit belongs to a specific book
    public string  Name        { get; set; } = string.Empty;
    public int?    Number      { get; set; }
    public int     SortOrder   { get; set; }
    public string? Description { get; set; }
    public string? Source      { get; set; }

    public Grade?            Grade      { get; set; }
    public Subject?          Subject    { get; set; }
    public Book?             Book       { get; set; }
    public ICollection<Topic>             Topics     { get; set; } = [];
    public ICollection<LearningObjective> Objectives { get; set; } = [];
}

public class Topic : AuditableEntity
{
    public int    UnitId    { get; set; }
    public string Name      { get; set; } = string.Empty;
    public int    SortOrder { get; set; }

    public Unit?                          Unit       { get; set; }
    public ICollection<LearningObjective> Objectives { get; set; } = [];
    public ICollection<ContentItem>       Resources  { get; set; } = [];
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

public class Book : AuditableEntity
{
    public int     SubjectId    { get; set; }
    public string? GradeCode    { get; set; }
    public string  Title        { get; set; } = string.Empty;
    public string? TitleUr      { get; set; }
    public string? Author       { get; set; }
    public string? Publisher    { get; set; }
    public string? Edition      { get; set; }   // e.g. "2024" or "3rd"
    public string? Board        { get; set; }   // e.g. "BISE Quetta"
    public string? Medium       { get; set; }   // "english" | "urdu"
    public string? Description  { get; set; }
    public string? CoverUrl     { get; set; }
    public string? DownloadUrl  { get; set; }
    public int     SortOrder    { get; set; }
    public bool    IsActive     { get; set; } = true;

    public Subject?            Subject { get; set; }
    public Grade?              Grade   { get; set; }
    public ICollection<Unit>   Units   { get; set; } = [];
}

public class GradeBand : AuditableEntity
{
    public string Name      { get; set; } = string.Empty;
    public int    SortOrder { get; set; }
}

public class Medium : AuditableEntity
{
    public string Name      { get; set; } = string.Empty;  // value stored on Subject.Medium
    public string Label     { get; set; } = string.Empty;  // display label in dropdowns
    public int    SortOrder { get; set; }
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
