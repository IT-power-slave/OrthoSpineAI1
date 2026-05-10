using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> b)
    {
        b.ToTable("Clinics");
        b.HasKey(c => c.ClinicId);
        b.Property(c => c.Name).IsRequired().HasMaxLength(200);
        b.Property(c => c.Address).HasMaxLength(300);
    }
}
