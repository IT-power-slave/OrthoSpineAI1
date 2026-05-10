using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.ToTable("Patients");
        b.HasKey(p => p.PatientId);
        b.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        b.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        b.Property(p => p.PESEL).HasMaxLength(11);
        b.HasIndex(p => p.PESEL).IsUnique();
        b.Property(p => p.AddressSt).HasMaxLength(200);
        b.Property(p => p.AddressCity).HasMaxLength(100);
        b.Property(p => p.ZipCode).HasMaxLength(10);
        b.HasOne(p => p.Clinic)
            .WithMany(c => c.Patients)
            .HasForeignKey(p => p.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
