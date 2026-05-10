using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Application.DTOs;

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
