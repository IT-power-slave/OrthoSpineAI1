namespace OrthoSpineAI.Domain.Exceptions;

/// <summary>
/// Thrown when a requested patient record does not exist in the data store.
/// </summary>
public sealed class PatientNotFoundException : Exception
{
    public int PatientId { get; }

    public PatientNotFoundException(int patientId)
        : base($"Patient with ID {patientId} was not found.")
    {
        PatientId = patientId;
    }
}
