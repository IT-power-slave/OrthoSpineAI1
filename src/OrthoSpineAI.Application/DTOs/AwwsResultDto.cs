namespace OrthoSpineAI.Application.DTOs;

/// <summary>Result of an AWWS evaluation for a completed medical examination.</summary>
public record AwwsResultDto(
    int MedTestId,
    int PatientId,
    DateTime ExaminationDate,
    string SurveyName,
    int PilsVariant,
    int PilsControlKey,
    string Conclusion,
    string ControlRecommendation,
    IReadOnlyDictionary<string, bool> GroupResults);
