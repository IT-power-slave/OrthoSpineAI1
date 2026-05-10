using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence.Configurations;

public class MedTestConfiguration : IEntityTypeConfiguration<MedTest>
{
    public void Configure(EntityTypeBuilder<MedTest> b)
    {
        b.ToTable("MedTests");
        b.HasKey(t => t.MedTestId);
        b.Property(t => t.ExaminationDate).IsRequired();
        b.Property(t => t.Description).HasMaxLength(1000);
        b.Property(t => t.MedTestDefinitionKey).IsRequired().HasMaxLength(100);
        b.HasOne(t => t.Patient)
            .WithMany(p => p.MedTests)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(t => t.SystemUser)
            .WithMany(u => u.MedTests)
            .HasForeignKey(t => t.SystemUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
