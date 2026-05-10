using OrthoSpineAI.Application.DTOs;

namespace OrthoSpineAI.Application.Interfaces;

public interface ISurveyService
{
    Task<IReadOnlyList<SurveyDefinitionDto>> GetAllDefinitionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SurveyDefinitionDto>> GetSurveyGroupAsync(string keyPrefix, CancellationToken ct = default);
}
