using Microsoft.EntityFrameworkCore;
using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<SystemUser> SystemUsers => Set<SystemUser>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedTest> MedTests => Set<MedTest>();
    public DbSet<MedTestResult> MedTestResults => Set<MedTestResult>();
    public DbSet<MedTestContinuousResult> MedTestContinuousResults => Set<MedTestContinuousResult>();
    public DbSet<MedTestDefinition> MedTestDefinitions => Set<MedTestDefinition>();
    public DbSet<MedTestStage> MedTestStages => Set<MedTestStage>();
    public DbSet<AwwsResult> AwwsResults => Set<AwwsResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
