using OrthoSpineAI.Domain.Enums;

namespace OrthoSpineAI.Domain.Entities;

public class Patient
{
    public int PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PESEL { get; set; } = string.Empty;
    public PatientSex Sex { get; set; }
    public DateTime BirthDate { get; set; }
    public string AddressSt { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public ICollection<MedTest> MedTests { get; set; } = new List<MedTest>();

    public string FullName => $"{FirstName} {LastName}";

    public int AgeYears => DateTime.Today.Year - BirthDate.Year
        - (DateTime.Today.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
}
