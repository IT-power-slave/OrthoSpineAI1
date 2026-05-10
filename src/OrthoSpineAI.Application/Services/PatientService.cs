using Microsoft.Extensions.Logging;
using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Exceptions;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;
    private readonly ILogger<PatientService> _logger;

    public PatientService(IPatientRepository repo, ILogger<PatientService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PatientDto>> GetAllAsync(CancellationToken ct = default)
    {
        var patients = await _repo.GetAllAsync(ct);
        return patients.Select(MapToDto).ToList();
    }

    public async Task<PatientDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        if (p is null)
            _logger.LogDebug("Patient with ID {PatientId} not found", id);
        return p is null ? null : MapToDto(p);
    }

    public async Task<PatientDto?> GetByPeselAsync(string pesel, CancellationToken ct = default)
    {
        var p = await _repo.GetByPeselAsync(pesel, ct);
        return p is null ? null : MapToDto(p);
    }

    public async Task<PatientDto> CreateAsync(PatientDto dto, CancellationToken ct = default)
    {
        var existing = await _repo.GetByPeselAsync(dto.PESEL, ct);
        if (existing is not null)
        {
            _logger.LogWarning("Duplicate PESEL on create: {Pesel}", dto.PESEL);
            throw new DuplicatePeselException(dto.PESEL);
        }
        var entity = MapToEntity(dto);
        await _repo.AddAsync(entity, ct);
        _logger.LogInformation("Patient created: {PatientId} ({FullName})", entity.PatientId, $"{dto.FirstName} {dto.LastName}");
        return MapToDto(entity);
    }

    public async Task UpdateAsync(PatientDto dto, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(dto.PatientId, ct);
        if (existing is null)
        {
            _logger.LogWarning("Update failed — patient not found: {PatientId}", dto.PatientId);
            throw new PatientNotFoundException(dto.PatientId);
        }
        var entity = MapToEntity(dto);
        await _repo.UpdateAsync(entity, ct);
        _logger.LogInformation("Patient updated: {PatientId}", dto.PatientId);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
        {
            _logger.LogWarning("Delete failed — patient not found: {PatientId}", id);
            throw new PatientNotFoundException(id);
        }
        await _repo.DeleteAsync(id, ct);
        _logger.LogInformation("Patient deleted: {PatientId}", id);
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
