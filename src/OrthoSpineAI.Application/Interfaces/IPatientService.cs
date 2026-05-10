using OrthoSpineAI.Application.DTOs;

namespace OrthoSpineAI.Application.Interfaces;

public interface IPatientService
{
    Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken ct = default);
    Task<PatientDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PatientDto?> GetByPeselAsync(string pesel, CancellationToken ct = default);
    Task<PatientDto> CreateAsync(PatientDto dto, CancellationToken ct = default);
    Task UpdateAsync(PatientDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PatientDto>> SearchAsync(string text, CancellationToken ct = default);
}
