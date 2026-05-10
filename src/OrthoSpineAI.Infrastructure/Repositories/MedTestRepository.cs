using Microsoft.EntityFrameworkCore;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;
using OrthoSpineAI.Infrastructure.Persistence;

namespace OrthoSpineAI.Infrastructure.Repositories;

public class MedTestRepository : IMedTestRepository
{
    private readonly AppDbContext _db;
    public MedTestRepository(AppDbContext db) => _db = db;

    public async Task<MedTest?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.MedTests
            .Include(t => t.Results)
            .Include(t => t.ContinuousResults)
            .FirstOrDefaultAsync(t => t.MedTestId == id, ct);

    public async Task<IReadOnlyList<MedTest>> GetByPatientIdAsync(int patientId, CancellationToken ct = default) =>
        await _db.MedTests
            .Where(t => t.PatientId == patientId)
            .OrderByDescending(t => t.ExaminationDate)
            .ToListAsync(ct);

    public async Task AddAsync(MedTest test, CancellationToken ct = default)
    {
        _db.MedTests.Add(test);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MedTest test, CancellationToken ct = default)
    {
        _db.MedTests.Update(test);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddResultAsync(MedTestResult result, CancellationToken ct = default) =>
        _db.MedTestResults.Add(result);

    public async Task AddContinuousResultAsync(MedTestContinuousResult result, CancellationToken ct = default) =>
        _db.MedTestContinuousResults.Add(result);

    public async Task SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);

    public async Task SaveAwwsResultAsync(AwwsResult result, CancellationToken ct = default)
    {
        _db.AwwsResults.Add(result);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AwwsResult?> GetAwwsResultAsync(int medTestId, CancellationToken ct = default) =>
        await _db.AwwsResults.FirstOrDefaultAsync(r => r.MedTestId == medTestId, ct);

    public async Task<IReadOnlyList<MedTest>> GetRecentAsync(int count, CancellationToken ct = default) =>
        await _db.MedTests
            .Include(t => t.Patient)
            .OrderByDescending(t => t.ExaminationDate)
            .Take(count)
            .ToListAsync(ct);

    public async Task<int> CountTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _db.MedTests.CountAsync(t => t.ExaminationDate >= today, ct);
    }

    public async Task<int> CountThisMonthAsync(CancellationToken ct = default)
    {
        var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return await _db.MedTests.CountAsync(t => t.ExaminationDate >= firstOfMonth, ct);
    }
}
