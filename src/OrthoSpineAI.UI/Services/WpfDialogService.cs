using OrthoSpineAI.Application.Interfaces;
using System.Windows;

namespace OrthoSpineAI.UI.Services;

/// <summary>
/// WPF implementation of <see cref="IDialogService"/>.
/// Wraps <see cref="MessageBox"/> so ViewModels stay free of WPF references.
/// </summary>
public sealed class WpfDialogService : IDialogService
{
    public bool Confirm(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning)
            == MessageBoxResult.Yes;

    public void ShowInfo(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message, string title) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
}
