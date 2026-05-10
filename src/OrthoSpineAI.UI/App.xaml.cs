using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrthoSpineAI.Application.Services;
using OrthoSpineAI.Infrastructure;
using OrthoSpineAI.Infrastructure.Persistence;
using OrthoSpineAI.UI.ViewModels;
using System.IO;
using System.Windows;

namespace OrthoSpineAI.UI;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OrthoSpineAI", "ortho.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var services = new ServiceCollection();

        // Infrastructure — Singleton DbContext is safe for a single-user desktop app
        services.AddInfrastructure(dbPath);

        // Application services — Singleton so ShellViewModel (Singleton) can consume them
        services.AddSingleton<AuthService>();
        services.AddSingleton<PatientService>();
        services.AddSingleton<SurveyService>();
        services.AddSingleton<MedTestService>();

        // ViewModels
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<MainViewModel>();

        _services = services.BuildServiceProvider();

        // Apply any pending migrations and seed reference data
        var db = _services.GetRequiredService<AppDbContext>();

        // If the database was previously created with EnsureCreated() it will have
        // the schema but no __EFMigrationsHistory table, causing MigrateAsync() to
        // fail with "table already exists". Detect this and recreate cleanly.
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        bool historyExists;
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
            historyExists = (long)(await cmd.ExecuteScalarAsync())! > 0;
        }
        await conn.CloseAsync();

        if (!historyExists || !db.Database.GetAppliedMigrations().Any())
        {
            // Schema exists without complete migration tracking — wipe and let migrations recreate it.
            await db.Database.EnsureDeletedAsync();
        }

        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(db);

        var mainVm = _services.GetRequiredService<MainViewModel>();
        var window = new MainWindow { DataContext = mainVm };
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}

