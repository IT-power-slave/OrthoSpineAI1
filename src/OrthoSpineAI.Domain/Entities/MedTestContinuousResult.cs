using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Domain.Entities;

/// <summary>
/// Stores one streaming telemetry frame from the ORT100 device.
/// Used primarily during the Adams test (OrtContinousMeas = true stages).
/// </summary>
public class MedTestContinuousResult
{
    public int MedTestContinuousResultId { get; set; }
    public int Status { get; set; }
    public int Signal { get; set; }
    public double Battery { get; set; }
    public double Shake { get; set; }
    public double Roll { get; set; }
    public double RollOffset { get; set; }
    public double Tilt { get; set; }
    public int Way { get; set; }
    public int Space { get; set; }
    public double Force1 { get; set; }
    public double Force2 { get; set; }
    public ORT100Measurement OrtMeas { get; set; }
    public DateTime Timestamp { get; set; }

    public int MedTestId { get; set; }
    public MedTest MedTest { get; set; } = null!;
}
