namespace OrthoSpineAI.Domain.Reports;

/// <summary>
/// A single measured or computed parameter entry within a <see cref="IParametersGroup"/>.
/// </summary>
/// <param name="Key">Algorithm parameter key (e.g. "ATR", "LL", "BEIGHTON").</param>
/// <param name="Label">Human-readable label shown in the report.</param>
/// <param name="Value">Formatted value string (e.g. "12°", "Tak", "3").</param>
public record ParameterEntry(string Key, string Label, string Value);
