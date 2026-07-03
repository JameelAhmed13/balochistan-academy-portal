using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalochiAcademy.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).HasMaxLength(100).IsRequired();
        b.Property(e => e.Description).HasMaxLength(500);
        b.Property(e => e.Price).HasColumnType("decimal(10,2)");
        b.Property(e => e.IsActive).HasDefaultValue(true);
    }
}

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
        b.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        b.HasOne(e => e.Plan).WithMany(p => p.UserSubscriptions).HasForeignKey(e => e.PlanId);
        b.HasIndex(e => e.UserId);
    }
}
