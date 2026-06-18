using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User>               Users               { get; }
    DbSet<Role>               Roles               { get; }
    DbSet<Permission>         Permissions         { get; }
    DbSet<RolePermission>     RolePermissions     { get; }
    DbSet<RefreshToken>       RefreshTokens       { get; }
    DbSet<LoginHistory>       LoginHistories      { get; }
    DbSet<Grade>              Grades              { get; }
    DbSet<Subject>            Subjects            { get; }
    DbSet<GradeSubject>       GradeSubjects       { get; }
    DbSet<Unit>               Units               { get; }
    DbSet<Topic>              Topics              { get; }
    DbSet<LearningObjective>  LearningObjectives  { get; }
    DbSet<GradeBand>          GradeBands          { get; }
    DbSet<AiTutor>            AiTutors            { get; }
    DbSet<Question>           Questions           { get; }
    DbSet<Test>               Tests               { get; }
    DbSet<TestQuestion>       TestQuestions       { get; }
    DbSet<TestAttempt>        TestAttempts        { get; }
    DbSet<CoinLedger>         CoinLedgers         { get; }
    DbSet<PayoutAccount>      PayoutAccounts      { get; }
    DbSet<WithdrawalRequest>  WithdrawalRequests  { get; }
    DbSet<Notification>       Notifications       { get; }
    DbSet<UserNotification>   UserNotifications   { get; }
    DbSet<Complaint>          Complaints          { get; }
    DbSet<ContentItem>        ContentItems        { get; }
    DbSet<VideoCourse>        VideoCourses        { get; }
    DbSet<VideoLesson>        VideoLessons        { get; }
    DbSet<StudentNote>        StudentNotes        { get; }
    DbSet<StudentProgress>    StudentProgress     { get; }
    DbSet<AuditLog>           AuditLogs           { get; }
    DbSet<SystemSetting>      SystemSettings      { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
