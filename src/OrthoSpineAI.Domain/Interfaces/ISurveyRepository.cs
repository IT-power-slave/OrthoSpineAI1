using OrthoSpineAI.Domain.Entities;

namespace OrthoSpineAI.Domain.Interfaces;

public interface ISurveyRepository
{
    Task<IReadOnlyList<MedTestDefinition>> GetDefinitionsByKeyPrefixAsync(string keyPrefix, CancellationToken ct = default);
    Task<MedTestDefinition?> GetDefinitionByKeyAsync(string key, CancellationToken ct = default);
    Task<IReadOnlyList<MedTestDefinition>> GetAllDefinitionsAsync(CancellationToken ct = default);
}
