namespace OrthoSpineAI.Application.DTOs;

public record DashboardDto(
    int TotalPatients,
    int TestsToday,
    int TestsThisMonth,
    IReadOnlyList<RecentTestDto> RecentTests);

public record RecentTestDto(
    int MedTestId,
    int PatientId,
    string PatientFullName,
    DateTime ExaminationDate,
    string SurveyName,
    int PilsVariant);
