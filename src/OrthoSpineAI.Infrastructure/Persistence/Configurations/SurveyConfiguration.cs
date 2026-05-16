using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence.Configurations;

public class MedTestDefinitionConfiguration : IEntityTypeConfiguration<MedTestDefinition>
{
    public void Configure(EntityTypeBuilder<MedTestDefinition> b)
    {
        b.ToTable("MedTestDefinitions");
        b.HasKey(d => d.MedTestDefinitionId);
        b.Property(d => d.Key).IsRequired().HasMaxLength(100);
        b.HasIndex(d => d.Key).IsUnique();
        b.Property(d => d.Name).IsRequired().HasMaxLength(200);
    }
}

public class MedTestStageConfiguration : IEntityTypeConfiguration<MedTestStage>
{
    public void Configure(EntityTypeBuilder<MedTestStage> b)
    {
        b.ToTable("MedTestStages");
        b.HasKey(s => s.MedTestStageId);
        b.Property(s => s.SortOrder);
        b.Property(s => s.Name).IsRequired().HasMaxLength(200);
        b.Property(s => s.Tip).HasMaxLength(2000);
        b.Property(s => s.TipControl).HasMaxLength(100);
        b.Property(s => s.MainSurveyControl).HasMaxLength(100);
        b.Property(s => s.BodyPlaneName).HasMaxLength(100);
        b.HasOne(s => s.MedTestDefinition)
            .WithMany(d => d.Stages)
            .HasForeignKey(s => s.MedTestDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
