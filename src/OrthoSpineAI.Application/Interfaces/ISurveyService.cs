using OrthoSpineAI.Application.DTOs;

namespace OrthoSpineAI.Application.Interfaces;

/// <summary>
/// Provides read access to survey/examination definitions.
/// </summary>
public interface ISurveyService
{
    /// <summary>Returns all top-level survey definitions available for selection.</summary>
    Task<IReadOnlyList<SurveyDefinitionDto>> GetAllDefinitionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns all sub-definitions whose key starts with <paramref name="keyPrefix"/>,
    /// used to expand a grouped examination into its individual stages.
    /// </summary>
    Task<IReadOnlyList<SurveyDefinitionDto>> GetSurveyGroupAsync(string keyPrefix, CancellationToken ct = default);
}
