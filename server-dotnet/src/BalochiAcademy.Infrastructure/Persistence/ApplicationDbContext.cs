using BalochiAcademy.Application.Common.Interfaces;
using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BalochiAcademy.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User>              Users              => Set<User>();
    public DbSet<Institute>         Institutes         => Set<Institute>();
    public DbSet<Role>              Roles              => Set<Role>();
    public DbSet<Permission>        Permissions        => Set<Permission>();
    public DbSet<RolePermission>    RolePermissions    => Set<RolePermission>();
    public DbSet<RefreshToken>      RefreshTokens      => Set<RefreshToken>();
    public DbSet<LoginHistory>      LoginHistories     => Set<LoginHistory>();
    public DbSet<Grade>             Grades             => Set<Grade>();
    public DbSet<Subject>           Subjects           => Set<Subject>();
    public DbSet<GradeSubject>      GradeSubjects      => Set<GradeSubject>();
    public DbSet<Unit>              Units              => Set<Unit>();
    public DbSet<Topic>             Topics             => Set<Topic>();
    public DbSet<LearningObjective> LearningObjectives => Set<LearningObjective>();
    public DbSet<GradeBand>         GradeBands         => Set<GradeBand>();
    public DbSet<Medium>            Mediums            => Set<Medium>();
    public DbSet<AiTutor>           AiTutors           => Set<AiTutor>();
    public DbSet<Book>              Books              => Set<Book>();
    public DbSet<Question>          Questions          => Set<Question>();
    public DbSet<Test>              Tests              => Set<Test>();
    public DbSet<TestQuestion>      TestQuestions      => Set<TestQuestion>();
    public DbSet<TestAttempt>       TestAttempts       => Set<TestAttempt>();
    public DbSet<CoinLedger>        CoinLedgers        => Set<CoinLedger>();
    public DbSet<PayoutAccount>     PayoutAccounts     => Set<PayoutAccount>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();
    public DbSet<Notification>         Notifications         => Set<Notification>();
    public DbSet<UserNotification>     UserNotifications     => Set<UserNotification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<Complaint>         Complaints         => Set<Complaint>();
    public DbSet<ContentItem>       ContentItems       => Set<ContentItem>();
    public DbSet<VideoCourse>       VideoCourses       => Set<VideoCourse>();
    public DbSet<VideoLesson>       VideoLessons       => Set<VideoLesson>();
    public DbSet<StudentNote>       StudentNotes       => Set<StudentNote>();
    public DbSet<StudentProgress>   StudentProgress    => Set<StudentProgress>();
    public DbSet<AuditLog>          AuditLogs          => Set<AuditLog>();
    public DbSet<SystemSetting>     SystemSettings     => Set<SystemSetting>();
    public DbSet<SubscriptionPlan>  SubscriptionPlans  => Set<SubscriptionPlan>();
    public DbSet<UserSubscription>  UserSubscriptions  => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
