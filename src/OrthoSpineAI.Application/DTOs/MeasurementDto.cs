using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Application.DTOs;

public record MedTestResultDto(
    int MedTestResultId,
    MedTestPlane Plane,
    ORT100Measurement OrtMeas,
    double PhysicalValue,
    string PhysicalUnit,
    MedTestSide Side,
    int MedTestId);

public record SaveMeasurementDto(
    int MedTestId,
    MedTestPlane Plane,
    ORT100Measurement OrtMeas,
    double PhysicalValue,
    string PhysicalUnit,
    MedTestSide Side);

public record SaveContinuousFrameDto(
    int MedTestId,
    ORT100Measurement OrtMeas,
    int Status,
    int Signal,
    double Battery,
    double Shake,
    double Roll,
    double RollOffset,
    double Tilt,
    int Way,
    int Space,
    double Force1,
    double Force2);
