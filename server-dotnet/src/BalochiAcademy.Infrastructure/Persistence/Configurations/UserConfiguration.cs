using BalochiAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BalochiAcademy.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Username).IsUnique();
        b.Property(e => e.Username).HasMaxLength(100).IsRequired();
        b.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
        b.Property(e => e.Medium).HasMaxLength(20).HasDefaultValue("English");
        b.Property(e => e.Board).HasMaxLength(100).HasDefaultValue("Balochistan");
        b.Property(e => e.GradeCode).HasMaxLength(10);
        b.Property(e => e.Phone).HasMaxLength(20);
        b.Property(e => e.Email).HasMaxLength(150);
        b.Property(e => e.Name).HasMaxLength(150);

        b.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId);
        b.HasOne(e => e.Grade).WithMany(g => g.Users).HasForeignKey(e => e.GradeCode);
        b.HasOne(e => e.Institute).WithMany(i => i.Users).HasForeignKey(e => e.InstituteId)
         .OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.PayoutAccount).WithOne(p => p.User)
         .HasForeignKey<PayoutAccount>(p => p.UserId);

        b.HasIndex(e => e.GradeCode);
        b.HasIndex(e => e.RoleId);
        b.HasIndex(e => e.InstituteId);
    }
}

public class InstituteConfiguration : IEntityTypeConfiguration<Institute>
{
    public void Configure(EntityTypeBuilder<Institute> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Name).IsUnique();
        b.Property(e => e.Name).HasMaxLength(200).IsRequired();
        b.Property(e => e.Code).HasMaxLength(20);
        b.Property(e => e.Address).HasMaxLength(500);
        b.Property(e => e.IsActive).HasDefaultValue(true);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Name).IsUnique();
        b.Property(e => e.Name).HasMaxLength(50).IsRequired();
        b.Property(e => e.Description).HasMaxLength(255);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Name).IsUnique();
        b.Property(e => e.Name).HasMaxLength(100).IsRequired();
        b.Property(e => e.Module).HasMaxLength(50).IsRequired();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.HasKey(e => new { e.RoleId, e.PermissionId });
        b.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId);
        b.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.Token).IsUnique();
        b.Property(e => e.Token).HasMaxLength(500).IsRequired();
        b.HasOne(e => e.User).WithMany(u => u.RefreshTokens).HasForeignKey(e => e.UserId);
    }
}
