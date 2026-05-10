using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Application.DTOs;

/// <summary>Read-only projection of a persisted examination record.</summary>
public record MedTestDto(
    int MedTestId,
    DateTime ExaminationDate,
    string Description,
    string MedTestDefinitionKey,
    double Weight,
    double Growth,
    int Beighton,
    bool TestPP,
    bool KneeValgus,
    bool TarsalValgus,
    bool GaitDisturbance,
    int PatientId,
    int SystemUserId);

/// <summary>Input DTO used to create a new examination record.</summary>
public record CreateMedTestDto(
    string MedTestDefinitionKey,
    double Weight,
    double Growth,
    int Beighton,
    bool TestPP,
    bool KneeValgus,
    bool TarsalValgus,
    bool GaitDisturbance,
    int PatientId,
    int SystemUserId,
    string Description = "");
