namespace OrthoSpineAI.Domain.Reports;

/// <summary>
/// A named group of diagnostic parameters within a <see cref="DiagnosticForm"/>.
/// Represents one PG-Logic group (e.g. ATR/HS, FLLD, LegsStatics) with its constituent
/// parameter values and its overall activation state.
/// </summary>
public interface IParametersGroup
{
    /// <summary>Internal algorithm group name (e.g. "PGLogicAtr", "PGLogicFLLD").</summary>
    string GroupName { get; }

    /// <summary>Human-readable Polish label shown in printed reports.</summary>
    string DisplayLabel { get; }

    /// <summary>Whether this group was flagged as active/positive by the AWWS engine.</summary>
    bool IsActive { get; }

    /// <summary>Individual parameter rows belonging to this group.</summary>
    IReadOnlyList<ParameterEntry> Parameters { get; }
}
