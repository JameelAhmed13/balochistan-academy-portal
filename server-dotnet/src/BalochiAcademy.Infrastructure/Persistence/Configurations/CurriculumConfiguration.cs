using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalochiAcademy.Infrastructure.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> b)
    {
        b.HasKey(e => e.Code);
        b.Property(e => e.Code).HasMaxLength(10);
        b.Property(e => e.Label).HasMaxLength(50).IsRequired();
        b.Property(e => e.Band).HasMaxLength(20).IsRequired();
    }
}

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Name).IsUnique();
        b.Property(e => e.Name).HasMaxLength(100).IsRequired();
        b.Property(e => e.NameUr).HasMaxLength(100);
        b.Property(e => e.Medium).HasMaxLength(20).HasDefaultValue("english");
    }
}

public class GradeSubjectConfiguration : IEntityTypeConfiguration<GradeSubject>
{
    public void Configure(EntityTypeBuilder<GradeSubject> b)
    {
        b.HasKey(e => new { e.GradeCode, e.SubjectId });
        b.HasOne(e => e.Grade).WithMany(g => g.GradeSubjects).HasForeignKey(e => e.GradeCode);
        b.HasOne(e => e.Subject).WithMany(s => s.GradeSubjects).HasForeignKey(e => e.SubjectId);
    }
}

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).HasMaxLength(200).IsRequired();
        b.Property(e => e.GradeCode).HasMaxLength(10).IsRequired();
        b.HasOne(e => e.Grade).WithMany(g => g.Units).HasForeignKey(e => e.GradeCode);
        b.HasOne(e => e.Subject).WithMany(s => s.Units).HasForeignKey(e => e.SubjectId);
        b.HasIndex(e => new { e.GradeCode, e.SubjectId });
    }
}

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).HasMaxLength(200).IsRequired();
        b.HasOne(e => e.Unit).WithMany(u => u.Topics).HasForeignKey(e => e.UnitId);
        b.HasIndex(e => e.UnitId);
    }
}

public class LearningObjectiveConfiguration : IEntityTypeConfiguration<LearningObjective>
{
    public void Configure(EntityTypeBuilder<LearningObjective> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.ObjectiveText).IsRequired();
        // Unit cascade is fine; Topic must be NO ACTION to avoid multiple cascade paths
        b.HasOne(e => e.Unit).WithMany(u => u.Objectives)
         .HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Topic).WithMany(t => t.Objectives)
         .HasForeignKey(e => e.TopicId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class GradeBandConfiguration : IEntityTypeConfiguration<GradeBand>
{
    public void Configure(EntityTypeBuilder<GradeBand> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Name).IsUnique();
        b.Property(e => e.Name).HasMaxLength(50).IsRequired();
        b.ToTable("GradeBands");
    }
}

public class MediumConfiguration : IEntityTypeConfiguration<Medium>
{
    public void Configure(EntityTypeBuilder<Medium> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Name).IsUnique();
        b.Property(e => e.Name).HasMaxLength(50).IsRequired();
        b.Property(e => e.Label).HasMaxLength(100).IsRequired();
        b.ToTable("Mediums");
    }
}

public class AiTutorConfiguration : IEntityTypeConfiguration<AiTutor>
{
    public void Configure(EntityTypeBuilder<AiTutor> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Slug).IsUnique();
        b.Property(e => e.Slug).HasMaxLength(50).IsRequired();
        b.Property(e => e.Persona).HasMaxLength(100).IsRequired();
        // Grade + Subject FKs: no cascade so deleting a grade/subject doesn't nuke tutors
        b.HasOne(e => e.Grade).WithMany(g => g.AiTutors)
         .HasForeignKey(e => e.GradeCode).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.Subject).WithMany(s => s.AiTutors)
         .HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.ContentHash).IsUnique().HasFilter("[ContentHash] IS NOT NULL");
        b.Property(e => e.Kind).HasMaxLength(20).IsRequired();
        b.Property(e => e.Difficulty).HasMaxLength(20);
        b.Property(e => e.CognitiveLevel).HasMaxLength(50);
        b.Property(e => e.GradeCode).HasMaxLength(10);
        b.HasIndex(e => new { e.GradeCode, e.SubjectId, e.UnitId });
        b.HasIndex(e => e.Kind);
    }
}

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Title).HasMaxLength(200).IsRequired();
        b.Property(e => e.Kind).HasMaxLength(30).HasDefaultValue("objective");
        b.HasIndex(e => e.GradeCode);
        b.HasIndex(e => e.Kind);
    }
}

public class TestQuestionConfiguration : IEntityTypeConfiguration<TestQuestion>
{
    public void Configure(EntityTypeBuilder<TestQuestion> b)
    {
        b.HasKey(e => new { e.TestId, e.QuestionId });
        b.HasOne(e => e.Test).WithMany(t => t.TestQuestions).HasForeignKey(e => e.TestId);
        b.HasOne(e => e.Question).WithMany(q => q.TestQuestions).HasForeignKey(e => e.QuestionId);
    }
}

public class TestAttemptConfiguration : IEntityTypeConfiguration<TestAttempt>
{
    public void Configure(EntityTypeBuilder<TestAttempt> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Percent).HasPrecision(5, 2);
        b.Property(e => e.AttemptType).HasMaxLength(30);
        b.HasOne(e => e.User).WithMany(u => u.TestAttempts).HasForeignKey(e => e.UserId);
        b.HasIndex(e => e.UserId);
        b.HasIndex(e => e.GradeCode);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> b)
    {
        b.HasKey(e => e.Key);
        b.Property(e => e.Key).HasMaxLength(100);
    }
}
