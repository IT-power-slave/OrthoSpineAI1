using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrthoSpineAI.Domain.Interfaces;
using OrthoSpineAI.Domain.Models;
using OrthoSpineAI.Infrastructure.Devices;
using OrthoSpineAI.Infrastructure.Persistence;
using OrthoSpineAI.Infrastructure.Repositories;

namespace OrthoSpineAI.Infrastructure;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers all Infrastructure services: EF Core (SQLite), repositories, and device driver.
    /// All registered as Singleton — safe for a single-user WPF desktop application.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string dbPath)
    {
        services.AddDbContext<AppDbContext>(
            opt => opt.UseSqlite($"Data Source={dbPath}"),
            contextLifetime: ServiceLifetime.Singleton,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddSingleton<IPatientRepository, PatientRepository>();
        services.AddSingleton<IMedTestRepository, MedTestRepository>();
        services.AddSingleton<ISurveyRepository, SurveyRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();

        services.AddSingleton<IDeviceDriver, BleDeviceDriver>();

        return services;
    }
}
