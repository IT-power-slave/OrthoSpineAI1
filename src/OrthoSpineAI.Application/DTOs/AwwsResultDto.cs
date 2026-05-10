namespace OrthoSpineAI.Application.DTOs;

public record AwwsResultDto(
    int MedTestId,
    DateTime ExaminationDate,
    string SurveyName,
    int PilsVariant,
    int PilsControlKey,
    string Conclusion,
    string ControlRecommendation,
    IReadOnlyDictionary<string, bool> GroupResults);
