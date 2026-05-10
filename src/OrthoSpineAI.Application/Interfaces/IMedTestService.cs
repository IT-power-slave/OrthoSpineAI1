using OrthoSpineAI.Application.DTOs;

namespace OrthoSpineAI.Application.Interfaces;

public interface IMedTestService
{
    Task<MedTestDto> CreateAsync(CreateMedTestDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<MedTestDto>> GetByPatientAsync(int patientId, CancellationToken ct = default);
    Task SaveMeasurementAsync(SaveMeasurementDto dto, CancellationToken ct = default);
    Task SaveContinuousFrameAsync(SaveContinuousFrameDto dto, CancellationToken ct = default);
    Task<AwwsResultDto> FinishTestAsync(int medTestId, int patientAgeYears, CancellationToken ct = default);
    Task<AwwsResultDto?> GetAwwsResultAsync(int medTestId, CancellationToken ct = default);
    Task<DashboardDto> GetDashboardAsync(int patientCount, CancellationToken ct = default);
}
