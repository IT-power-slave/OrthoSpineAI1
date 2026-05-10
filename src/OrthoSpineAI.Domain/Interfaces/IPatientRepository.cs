using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Domain.Interfaces;

public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken ct = default);
    Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Patient?> GetByPeselAsync(string pesel, CancellationToken ct = default);
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task UpdateAsync(Patient patient, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
