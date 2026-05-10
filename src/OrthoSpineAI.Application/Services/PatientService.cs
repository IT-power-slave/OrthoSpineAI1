using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Services;

public class PatientService
{
    private readonly IPatientRepository _repo;

    public PatientService(IPatientRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken ct = default)
    {
        var patients = await _repo.GetAllAsync(ct);
        return patients.Select(MapToDto).ToList();
    }

    public async Task<PatientDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : MapToDto(p);
    }

    public async Task<PatientDto?> GetByPeselAsync(string pesel, CancellationToken ct = default)
    {
        var p = await _repo.GetByPeselAsync(pesel, ct);
        return p is null ? null : MapToDto(p);
    }

    public async Task<PatientDto> CreateAsync(PatientDto dto, CancellationToken ct = default)
    {
        var entity = MapToEntity(dto);
        await _repo.AddAsync(entity, ct);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(PatientDto dto, CancellationToken ct = default)
    {
        var entity = MapToEntity(dto);
        await _repo.UpdateAsync(entity, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(id, ct);
    }

    public async Task<IReadOnlyList<PatientDto>> SearchAsync(string text, CancellationToken ct = default)
    {
        var all = await _repo.GetAllAsync(ct);
        if (string.IsNullOrWhiteSpace(text))
            return all.Select(MapToDto).ToList();
        var lower = text.ToLowerInvariant();
        return all
            .Where(p => p.FirstName.Contains(lower, StringComparison.OrdinalIgnoreCase)
                     || p.LastName.Contains(lower, StringComparison.OrdinalIgnoreCase)
                     || p.PESEL.Contains(lower, StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto)
            .ToList();
    }

    private static PatientDto MapToDto(Patient p) => new(
        p.PatientId, p.FirstName, p.LastName, p.PESEL,
        p.Sex, p.BirthDate, p.AddressSt, p.AddressCity, p.ZipCode, p.ClinicId);

    private static Patient MapToEntity(PatientDto dto) => new()
    {
        PatientId = dto.PatientId,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        PESEL = dto.PESEL,
        Sex = dto.Sex,
        BirthDate = dto.BirthDate,
        AddressSt = dto.AddressSt,
        AddressCity = dto.AddressCity,
        ZipCode = dto.ZipCode,
        ClinicId = dto.ClinicId
    };
}
