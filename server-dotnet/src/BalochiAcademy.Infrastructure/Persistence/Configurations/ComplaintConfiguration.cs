using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalochiAcademy.Infrastructure.Persistence.Configurations;

public class ComplaintMessageConfiguration : IEntityTypeConfiguration<ComplaintMessage>
{
    public void Configure(EntityTypeBuilder<ComplaintMessage> b)
    {
        b.HasKey(m => m.Id);
        b.Property(m => m.Message).IsRequired();
        b.HasOne(m => m.Complaint).WithMany(c => c.Messages)
            .HasForeignKey(m => m.ComplaintId).OnDelete(DeleteBehavior.Cascade);
        // NoAction: users are deactivated, not deleted, in this app — and this sidesteps the
        // same multi-cascade-path issue hit earlier (Complaint.UserId already reaches Users).
        b.HasOne(m => m.Sender).WithMany()
            .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(m => m.ComplaintId);
    }
}
