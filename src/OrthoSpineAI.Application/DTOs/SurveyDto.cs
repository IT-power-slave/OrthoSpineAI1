using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Application.DTOs;

public record StageDto(
    int MedTestStageId,
    string Name,
    string Tip,
    string TipControl,
    string MainSurveyControl,
    MedTestPlane Plane,
    ORT100Measurement OrtMeas,
    ORT100ControlState OrtState,
    ORT100Button OrtNextStepButton,
    ORT100Mode OrtMode,
    ORT100ResetFlag OrtResetFlag,
    bool OrtContinousMeas,
    double? ValueISOM1,
    double? ValueISOM3,
    int MedTestDefinitionId);

public record SurveyDefinitionDto(
    int MedTestDefinitionId,
    string Key,
    string Name,
    IReadOnlyList<StageDto> Stages);
