using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Domain.Entities;

/// <summary>
/// One step inside a survey template. All fields drive hardware configuration and UI rendering.
/// Ordering is determined by MedTestStageId (insertion order) — no separate SortOrder column.
/// </summary>
public class MedTestStage
{
    public int MedTestStageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tip { get; set; } = string.Empty;
    public string TipControl { get; set; } = string.Empty;
    public string MainSurveyControl { get; set; } = string.Empty;
    public string BodyPlaneName { get; set; } = string.Empty;
    public MedTestPlane Plane { get; set; }
    public ORT100Measurement OrtMeas { get; set; }
    public ORT100ControlState OrtState { get; set; }
    public ORT100Button OrtNextStepButton { get; set; }
    public ORT100Mode OrtMode { get; set; }
    public ORT100ResetFlag OrtResetFlag { get; set; }
    public bool OrtContinousMeas { get; set; }
    public double? ValueISOM1 { get; set; }
    public double? ValueISOM3 { get; set; }

    public int MedTestDefinitionId { get; set; }
    public MedTestDefinition MedTestDefinition { get; set; } = null!;
}
