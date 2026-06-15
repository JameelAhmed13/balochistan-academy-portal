using BalochiAcademy.Domain.Common;
using BalochiAcademy.Domain.Enums;

namespace BalochiAcademy.Domain.Entities;

public class Question : AuditableEntity
{
    public string?   GradeCode      { get; set; }
    public int?      SubjectId      { get; set; }
    public int?      UnitId         { get; set; }
    public int?      TopicId        { get; set; }
    public string    Kind           { get; set; } = "objective";  // 'objective' | 'subjective'
    public string    Stem           { get; set; } = string.Empty;
    public string?   OptionsJson    { get; set; }   // JSON ["A","B","C","D"]
    public int?      CorrectIndex   { get; set; }   // 0-based
    public string?   QuestionType   { get; set; }   // 'Short' | 'Long'
    public int?      Marks          { get; set; }
    public string?   ModelAnswer    { get; set; }
    public string?   TopicText      { get; set; }
    public string?   Skill          { get; set; }
    public string?   Difficulty     { get; set; }   // 'Easy' | 'Medium' | 'Hard'
    public string?   CognitiveLevel { get; set; }
    public string?   Feedback       { get; set; }
    public string?   Article        { get; set; }
    public string?   SourcePaper    { get; set; }
    public string?   SourceFormat   { get; set; }
    public bool      IsAiGenerated  { get; set; }
    public bool      IsEntranceExam { get; set; }
    public string?   SloCode        { get; set; }
    public string?   ContentHash    { get; set; }
    public int?      CreatedById    { get; set; }

    public Grade?    Grade    { get; set; }
    public Subject?  Subject  { get; set; }
    public Unit?     Unit     { get; set; }
    public Topic?    Topic    { get; set; }
    public User?     CreatedBy { get; set; }
    public ICollection<TestQuestion> TestQuestions { get; set; } = [];
}
