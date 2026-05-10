using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence.Configurations;

public class SystemUserConfiguration : IEntityTypeConfiguration<SystemUser>
{
    public void Configure(EntityTypeBuilder<SystemUser> b)
    {
        b.ToTable("SystemUsers");
        b.HasKey(u => u.SystemUserId);
        b.Property(u => u.Login).IsRequired().HasMaxLength(100);
        b.HasIndex(u => u.Login).IsUnique();
        b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
        b.HasOne(u => u.Clinic)
            .WithMany(c => c.SystemUsers)
            .HasForeignKey(u => u.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
