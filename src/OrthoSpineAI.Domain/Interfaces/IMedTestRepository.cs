using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Domain.Interfaces;

public interface IMedTestRepository
{
    Task<MedTest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MedTest>> GetByPatientIdAsync(int patientId, CancellationToken ct = default);
    Task AddAsync(MedTest test, CancellationToken ct = default);
    Task UpdateAsync(MedTest test, CancellationToken ct = default);
    Task AddResultAsync(MedTestResult result, CancellationToken ct = default);
    Task AddContinuousResultAsync(MedTestContinuousResult result, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    Task SaveAwwsResultAsync(AwwsResult result, CancellationToken ct = default);
    Task<AwwsResult?> GetAwwsResultAsync(int medTestId, CancellationToken ct = default);
    Task<IReadOnlyList<MedTest>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<int> CountTodayAsync(CancellationToken ct = default);
    Task<int> CountThisMonthAsync(CancellationToken ct = default);
}
