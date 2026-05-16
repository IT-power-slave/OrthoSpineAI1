namespace OrthoSpineAI.Domain.Reports;

/// <summary>
/// Concrete implementation of a diagnostic parameter group built from AWWS engine output.
/// </summary>
public sealed class ParametersGroup : IParametersGroup
{
    public string GroupName { get; }
    public string DisplayLabel { get; }
    public bool IsActive { get; }
    public IReadOnlyList<ParameterEntry> Parameters { get; }

    public ParametersGroup(
        string groupName,
        string displayLabel,
        bool isActive,
        IReadOnlyList<ParameterEntry> parameters)
    {
        GroupName    = groupName;
        DisplayLabel = displayLabel;
        IsActive     = isActive;
        Parameters   = parameters;
    }
}
