using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalochiAcademy.Infrastructure.Persistence.Configurations;

// ── Long-Id entities ─────────────────────────────────────────────────────────

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Status).HasMaxLength(20);
        b.Property(e => e.IpAddress).HasMaxLength(50);
        b.Property(e => e.UserAgent).HasMaxLength(500);
        b.Property(e => e.DeviceInfo).HasMaxLength(200);
        b.HasOne(e => e.User).WithMany(u => u.LoginHistories)
         .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.UserId);
    }
}

public class CoinLedgerConfiguration : IEntityTypeConfiguration<CoinLedger>
{
    public void Configure(EntityTypeBuilder<CoinLedger> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Reason).HasMaxLength(200);
        b.HasOne(e => e.User).WithMany(u => u.CoinLedger)
         .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Attempt).WithMany(a => a.CoinLedgers)
         .HasForeignKey(e => e.AttemptId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(e => e.UserId);
    }
}

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> b)
    {
        b.HasKey(e => e.Id);
        b.HasOne(e => e.User).WithMany(u => u.Notifications)
         .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Notification).WithMany(n => n.UserNotifications)
         .HasForeignKey(e => e.NotificationId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.UserId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Action).HasMaxLength(100).IsRequired();
        b.Property(e => e.EntityType).HasMaxLength(100);
        b.Property(e => e.IpAddress).HasMaxLength(50);
        // NoAction — audit logs outlive users
        b.HasOne(e => e.User).WithMany()
         .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(e => e.UserId);
        b.HasIndex(e => e.Timestamp);
        b.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}

// ── Multi-FK entities (avoid multiple cascade paths) ─────────────────────────

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("general");
        b.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("open");
        b.Property(e => e.Subject).HasMaxLength(200).IsRequired();
        b.HasOne(e => e.User).WithMany(u => u.Complaints)
         .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        // Second FK to Users — must be NoAction
        b.HasOne(e => e.HandledBy).WithMany()
         .HasForeignKey(e => e.HandledById).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(e => e.UserId);
        b.HasIndex(e => e.Status);
    }
}

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
        b.Property(e => e.AmountPkr).HasPrecision(10, 2);
        // NoAction: financial records must outlive the user; two cascade paths exist via PayoutAccounts
        b.HasOne(e => e.User).WithMany()
         .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(e => e.Account).WithMany(a => a.WithdrawalRequests)
         .HasForeignKey(e => e.AccountId).OnDelete(DeleteBehavior.SetNull);
        // Second FK to Users — must be NoAction
        b.HasOne(e => e.ProcessedBy).WithMany()
         .HasForeignKey(e => e.ProcessedById).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(e => e.UserId);
        b.HasIndex(e => e.Status);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Title).HasMaxLength(200).IsRequired();
        b.Property(e => e.Type).HasMaxLength(30).HasDefaultValue("info");
        b.Property(e => e.TargetRole).HasMaxLength(20);
        b.Property(e => e.TargetGrade).HasMaxLength(10);
        b.HasOne(e => e.CreatedBy).WithMany()
         .HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(e => e.Grade).WithMany()
         .HasForeignKey(e => e.TargetGrade).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Kind).HasMaxLength(30).IsRequired();
        b.Property(e => e.Title).HasMaxLength(300).IsRequired();
        b.HasOne(e => e.CreatedBy).WithMany()
         .HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(e => new { e.GradeCode, e.SubjectId });
        b.HasIndex(e => e.Kind);
    }
}

public class TestConfigurationExtra : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> b)
    {
        // CreatedById is a second FK to Users — override default cascade to NoAction
        b.HasOne(e => e.CreatedBy).WithMany()
         .HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.NoAction);
    }
}

public class QuestionConfigurationExtra : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> b)
    {
        b.HasOne(e => e.CreatedBy).WithMany()
         .HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.NoAction);
    }
}
