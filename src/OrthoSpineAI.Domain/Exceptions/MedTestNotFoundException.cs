namespace OrthoSpineAI.Domain.Exceptions;

/// <summary>
/// Thrown when a requested medical test record does not exist in the data store.
/// </summary>
public sealed class MedTestNotFoundException : Exception
{
    public int MedTestId { get; }

    public MedTestNotFoundException(int medTestId)
        : base($"Medical test with ID {medTestId} was not found.")
    {
        MedTestId = medTestId;
    }
}
