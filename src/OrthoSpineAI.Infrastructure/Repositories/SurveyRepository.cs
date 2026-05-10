using Microsoft.EntityFrameworkCore;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;
using OrthoSpineAI.Infrastructure.Persistence;

namespace OrthoSpineAI.Infrastructure.Repositories;

public class SurveyRepository : ISurveyRepository
{
    private readonly AppDbContext _db;
    public SurveyRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<MedTestDefinition>> GetAllDefinitionsAsync(CancellationToken ct = default) =>
        await _db.MedTestDefinitions
            .Include(d => d.Stages)
            .OrderBy(d => d.Key)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MedTestDefinition>> GetDefinitionsByKeyPrefixAsync(string keyPrefix, CancellationToken ct = default) =>
        await _db.MedTestDefinitions
            .Include(d => d.Stages)
            .Where(d => d.Key == keyPrefix || d.Key.StartsWith(keyPrefix + "."))
            .OrderBy(d => d.Key)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<MedTestDefinition?> GetDefinitionByKeyAsync(string key, CancellationToken ct = default) =>
        await _db.MedTestDefinitions
            .Include(d => d.Stages)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Key == key, ct);
}
