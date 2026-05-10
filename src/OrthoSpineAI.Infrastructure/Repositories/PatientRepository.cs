using Microsoft.EntityFrameworkCore;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;
using OrthoSpineAI.Infrastructure.Persistence;

namespace OrthoSpineAI.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _db;
    public PatientRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Patients.AsNoTracking().OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync(ct);

    public async Task<Patient?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == id, ct);

    public async Task<Patient?> GetByPeselAsync(string pesel, CancellationToken ct = default) =>
        await _db.Patients.FirstOrDefaultAsync(p => p.PESEL == pesel, ct);

    public async Task AddAsync(Patient patient, CancellationToken ct = default)
    {
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken ct = default)
    {
        _db.Patients.Update(patient);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var p = await _db.Patients.FindAsync(new object[] { id }, ct);
        if (p is not null)
        {
            _db.Patients.Remove(p);
            await _db.SaveChangesAsync(ct);
        }
    }
}
