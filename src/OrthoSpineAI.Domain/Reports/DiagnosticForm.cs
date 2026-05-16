namespace OrthoSpineAI.Domain.Reports;

/// <summary>
/// In-memory aggregate that represents a complete printable diagnostic report
/// for one examination session.
/// 
/// Structure (mirrors Appendix E of <c>Implementation_Guide.md</c>):
/// <code>
/// DiagnosticForm
///   └─ ParametersGroups[]
///         └─ IParametersGroup
///               └─ Parameters[]
/// </code>
/// </summary>
public sealed class DiagnosticForm
{
    // ── Session metadata ────────────────────────────────────────────────

    public int MedTestId { get; init; }
    public int PatientId { get; init; }
    public DateTime ExaminationDate { get; init; }
    public string SurveyName { get; init; } = string.Empty;
    public string PatientNotes { get; init; } = string.Empty;

    // ── Anthropometric inputs ───────────────────────────────────────────

    public double Weight { get; init; }
    public double Height { get; init; }
    public int AgeYears { get; init; }

    // ── AWWS outcome ────────────────────────────────────────────────────

    public int PilsVariant { get; init; }
    public int PilsControlKey { get; init; }
    public string Conclusion { get; init; } = string.Empty;
    public string ControlRecommendation { get; init; } = string.Empty;

    // ── Parameter groups ────────────────────────────────────────────────

    public IReadOnlyList<IParametersGroup> ParametersGroups { get; init; } = [];
}
