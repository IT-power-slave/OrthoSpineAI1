namespace OrthoSpineAI.UI.ViewModels;

/// <summary>
/// Legacy entry point — kept so MainWindow.xaml DataContext binding compiles.
/// The real navigation host is <see cref="ShellViewModel"/>.
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    public ShellViewModel Shell { get; }

    public MainViewModel(ShellViewModel shell)
    {
        Shell = shell;
    }

    public void Dispose() { }
}
