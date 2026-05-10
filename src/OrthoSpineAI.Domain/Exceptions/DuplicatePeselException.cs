namespace OrthoSpineAI.Domain.Exceptions;

/// <summary>
/// Thrown when an attempt is made to register a patient whose PESEL number
/// already belongs to an existing record.
/// </summary>
public sealed class DuplicatePeselException : Exception
{
    public string Pesel { get; }

    public DuplicatePeselException(string pesel)
        : base($"A patient with PESEL {pesel} already exists.")
    {
        Pesel = pesel;
    }
}
