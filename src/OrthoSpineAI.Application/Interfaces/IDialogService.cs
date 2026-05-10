namespace OrthoSpineAI.Application.Interfaces;

public interface IDialogService
{
    /// <summary>Shows a yes/no confirmation dialog. Returns true when the user confirms.</summary>
    bool Confirm(string message, string title);

    /// <summary>Shows an informational message dialog.</summary>
    void ShowInfo(string message, string title);

    /// <summary>Shows an error message dialog.</summary>
    void ShowError(string message, string title);
}
