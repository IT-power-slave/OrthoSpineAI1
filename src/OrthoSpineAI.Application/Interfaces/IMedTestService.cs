using OrthoSpineAI.Application.DTOs;

namespace OrthoSpineAI.Application.Interfaces;

/// <summary>
/// Manages the full lifecycle of a medical examination (MedTest): creation, measurement
/// recording, AWWS evaluation, and result retrieval.
/// </summary>
public interface IMedTestService
{
    /// <summary>Creates a new examination record and returns its persisted DTO.</summary>
    Task<MedTestDto> CreateAsync(CreateMedTestDto dto, CancellationToken ct = default);

    /// <summary>Returns all examinations recorded for <paramref name="patientId"/>.</summary>
    Task<IReadOnlyList<MedTestDto>> GetByPatientAsync(int patientId, CancellationToken ct = default);

    /// <summary>Persists a single discrete measurement frame for an ongoing examination.</summary>
    Task SaveMeasurementAsync(SaveMeasurementDto dto, CancellationToken ct = default);

    /// <summary>Persists a continuous (streaming) sensor frame for an ongoing examination.</summary>
    Task SaveContinuousFrameAsync(SaveContinuousFrameDto dto, CancellationToken ct = default);

    /// <summary>
    /// Closes the examination, runs the AWWS evaluation algorithm against the recorded
    /// measurements, persists the result, and returns the outcome DTO.
    /// </summary>
    Task<AwwsResultDto> FinishTestAsync(int medTestId, int patientAgeYears, CancellationToken ct = default);

    /// <summary>
    /// Returns the previously computed AWWS result for <paramref name="medTestId"/>, or
    /// <see langword="null"/> if no result has been persisted yet.
    /// </summary>
    Task<AwwsResultDto?> GetAwwsResultAsync(int medTestId, CancellationToken ct = default);

    /// <summary>
    /// Returns dashboard summary statistics. <paramref name="patientCount"/> is the total
    /// number of patients already loaded by the caller.
    /// </summary>
    Task<DashboardDto> GetDashboardAsync(int patientCount, CancellationToken ct = default);
}
