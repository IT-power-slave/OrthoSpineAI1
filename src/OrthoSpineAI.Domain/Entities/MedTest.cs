namespace OrthoSpineAI.Domain.Entities;

public class MedTest
{
    public int MedTestId { get; set; }
    public DateTime ExaminationDate { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>FK to MedTestDefinition.Key (e.g. "backbone").</summary>
    public string MedTestDefinitionKey { get; set; } = string.Empty;

    public MedTestDefinition? MedTestDefinition { get; set; }

    public double Weight { get; set; }

    /// <summary>Height in cm. Column named Growth per domain specification.</summary>
    public double Growth { get; set; }

    public int Beighton { get; set; }

    /// <summary>Hump Score (0–20) — back asymmetry measured with a point grid (AWWS §2).</summary>
    public int Hs { get; set; }

    public bool TestPP { get; set; }
    public bool KneeValgus { get; set; }
    public bool TarsalValgus { get; set; }
    public bool GaitDisturbance { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int SystemUserId { get; set; }
    public SystemUser SystemUser { get; set; } = null!;

    public ICollection<MedTestResult> Results { get; set; } = new List<MedTestResult>();
    public ICollection<MedTestContinuousResult> ContinuousResults { get; set; } = new List<MedTestContinuousResult>();
}
