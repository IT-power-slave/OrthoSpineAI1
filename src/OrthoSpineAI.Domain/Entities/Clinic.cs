namespace OrthoSpineAI.Domain.Entities;

public class Clinic
{
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<SystemUser> SystemUsers { get; set; } = new List<SystemUser>();
}
