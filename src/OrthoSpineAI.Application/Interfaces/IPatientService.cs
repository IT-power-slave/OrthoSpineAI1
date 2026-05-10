using OrthoSpineAI.Application.DTOs;

namespace OrthoSpineAI.Application.Interfaces;

/// <summary>
/// Application service for managing patient records.
/// </summary>
public interface IPatientService
{
    /// <summary>Returns all patients in the clinic.</summary>
    Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns the patient with the given primary key, or <see langword="null"/>.</summary>
    Task<PatientDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Returns the patient whose PESEL matches exactly, or <see langword="null"/>.</summary>
    Task<PatientDto?> GetByPeselAsync(string pesel, CancellationToken ct = default);

    /// <summary>Persists a new patient and returns the created record.</summary>
    Task<PatientDto> CreateAsync(PatientDto dto, CancellationToken ct = default);

    /// <summary>Updates an existing patient record.</summary>
    Task UpdateAsync(PatientDto dto, CancellationToken ct = default);

    /// <summary>Deletes the patient and all associated data for the given <paramref name="id"/>.</summary>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Returns patients whose name or PESEL contains <paramref name="text"/>.</summary>
    Task<IReadOnlyList<PatientDto>> SearchAsync(string text, CancellationToken ct = default);
}
