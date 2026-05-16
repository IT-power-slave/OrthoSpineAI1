using OrthoSpineAI.Application.DTOs;
using OrthoSpineAI.Application.Interfaces;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Interfaces;

namespace OrthoSpineAI.Application.Services;

public class SurveyService : ISurveyService
{
    private readonly ISurveyRepository _repo;

    public SurveyService(ISurveyRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<SurveyDefinitionDto>> GetAllDefinitionsAsync(CancellationToken ct = default)
    {
        var defs = await _repo.GetAllDefinitionsAsync(ct);
        // Return only root-level definitions (keys without a dot = top-level exam types)
        return defs
            .Where(d => !d.Key.Contains('.'))
            .Select(MapToDto)
            .ToList();
    }

    /// <summary>
    /// Loads all definitions whose Key starts with keyPrefix, ordered by Key.
    /// E.g. "backbone" returns backbone, backbone.1, backbone.2, backbone.summary.
    /// </summary>
    public async Task<IReadOnlyList<SurveyDefinitionDto>> GetSurveyGroupAsync(string keyPrefix, CancellationToken ct = default)
    {
        var defs = await _repo.GetDefinitionsByKeyPrefixAsync(keyPrefix, ct);
        return defs.Select(MapToDto).ToList();
    }

    private static SurveyDefinitionDto MapToDto(MedTestDefinition d) => new(
        d.MedTestDefinitionId,
        d.Key,
        d.Name,
        d.Stages.OrderBy(s => s.SortOrder).Select(MapStage).ToList());

    private static StageDto MapStage(MedTestStage s) => new(
        s.MedTestStageId, s.Name, s.Tip, s.TipControl, s.MainSurveyControl,
        s.Plane, s.OrtMeas, s.OrtState, s.OrtNextStepButton,
        s.OrtMode, s.OrtResetFlag, s.OrtContinousMeas,
        s.ValueISOM1, s.ValueISOM3, s.MedTestDefinitionId);
}
