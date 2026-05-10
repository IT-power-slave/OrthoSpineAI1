using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence.Configurations;

public class AwwsResultConfiguration : IEntityTypeConfiguration<AwwsResult>
{
    public void Configure(EntityTypeBuilder<AwwsResult> builder)
    {
        builder.HasKey(r => r.AwwsResultId);
        builder.Property(r => r.Conclusion).IsRequired().HasMaxLength(1000);
        builder.Property(r => r.ControlRecommendation).HasMaxLength(1000);
        builder.Property(r => r.GroupResultsJson).IsRequired().HasDefaultValue("{}");

        builder.HasOne(r => r.MedTest)
               .WithOne()
               .HasForeignKey<AwwsResult>(r => r.MedTestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
