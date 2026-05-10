namespace OrthoSpineAI.Domain.Entities;

public class MedTestDefinition
{
    public int MedTestDefinitionId { get; set; }

    /// <summary>Unique text key, e.g. "backbone.1".</summary>
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICollection<MedTestStage> Stages { get; set; } = new List<MedTestStage>();
}
