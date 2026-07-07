using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalochiAcademy.Infrastructure.Persistence.Configurations;

public class CoinRedemptionConfiguration : IEntityTypeConfiguration<CoinRedemption>
{
    public void Configure(EntityTypeBuilder<CoinRedemption> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Type).HasMaxLength(30).IsRequired();
        b.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        b.HasOne(e => e.Plan).WithMany().HasForeignKey(e => e.PlanId).OnDelete(DeleteBehavior.SetNull);
        // NoAction, not SetNull: UserSubscriptions.UserId already cascades from Users, and this FK
        // also chains through UserSubscriptions back to Users — two cascade paths to the same table
        // is something SQL Server refuses to create. Subscriptions are never hard-deleted anyway
        // (they're marked Status = "expired"), so NoAction never actually blocks a real delete.
        b.HasOne(e => e.Subscription).WithMany().HasForeignKey(e => e.SubscriptionId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(e => e.UserId);
        b.HasIndex(e => e.CreatedAt);
    }
}
