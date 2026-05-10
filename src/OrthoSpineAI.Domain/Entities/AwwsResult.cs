namespace OrthoSpineAI.Domain.Entities;

/// <summary>
/// Persisted AWWS/PiLS diagnostic result linked to a MedTest.
/// </summary>
public class AwwsResult
{
    public int AwwsResultId { get; set; }

    public int MedTestId { get; set; }
    public MedTest MedTest { get; set; } = null!;

    public int PilsVariant { get; set; }
    public int PilsControlKey { get; set; }
    public string Conclusion { get; set; } = string.Empty;
    public string ControlRecommendation { get; set; } = string.Empty;

    /// <summary>Serialised as JSON: {"GroupName":true/false, ...}</summary>
    public string GroupResultsJson { get; set; } = "{}";
}
