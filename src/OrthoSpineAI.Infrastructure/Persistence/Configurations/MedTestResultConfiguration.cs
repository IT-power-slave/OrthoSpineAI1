using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence.Configurations;

public class MedTestResultConfiguration : IEntityTypeConfiguration<MedTestResult>
{
    public void Configure(EntityTypeBuilder<MedTestResult> b)
    {
        b.ToTable("MedTestResults");
        b.HasKey(r => r.MedTestResultId);
        b.Property(r => r.PhysicalUnit).IsRequired().HasMaxLength(10);
        b.HasOne(r => r.MedTest)
            .WithMany(t => t.Results)
            .HasForeignKey(r => r.MedTestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MedTestContinuousResultConfiguration : IEntityTypeConfiguration<MedTestContinuousResult>
{
    public void Configure(EntityTypeBuilder<MedTestContinuousResult> b)
    {
        b.ToTable("MedTestContinuousResults");
        b.HasKey(r => r.MedTestContinuousResultId);
        b.Property(r => r.Timestamp).IsRequired();
        b.HasOne(r => r.MedTest)
            .WithMany(t => t.ContinuousResults)
            .HasForeignKey(r => r.MedTestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
