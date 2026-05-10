using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Application.DTOs;

/// <summary>Read-only projection of a patient record returned from the Application layer.</summary>
public record PatientDto(
    int PatientId,
    string FirstName,
    string LastName,
    string PESEL,
    PatientSex Sex,
    DateTime BirthDate,
    string AddressSt,
    string AddressCity,
    string ZipCode,
    int ClinicId)
{
    public string FullName => $"{FirstName} {LastName}";
    public int AgeYears => DateTime.Today.Year - BirthDate.Year
        - (DateTime.Today.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
}
