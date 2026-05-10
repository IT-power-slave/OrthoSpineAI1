namespace OrthoSpineAI.Domain.Entities;

public class SystemUser
{
    public int SystemUserId { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public ICollection<MedTest> MedTests { get; set; } = new List<MedTest>();
}
