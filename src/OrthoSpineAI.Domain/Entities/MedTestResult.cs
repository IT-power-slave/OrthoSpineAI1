using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Domain.Entities;

public class MedTestResult
{
    public int MedTestResultId { get; set; }
    public MedTestPlane Plane { get; set; }
    public ORT100Measurement OrtMeas { get; set; }
    public double PhysicalValue { get; set; }
    public string PhysicalUnit { get; set; } = "°";
    public MedTestSide Side { get; set; }

    public int MedTestId { get; set; }
    public MedTest MedTest { get; set; } = null!;
}
