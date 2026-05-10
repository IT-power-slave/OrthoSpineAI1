using System.Windows.Controls;

namespace OrthoSpineAI.UI.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        // Forward PasswordBox text to ViewModel (PasswordBox cannot bind for security reasons)
        PasswordBox.PasswordChanged += (_, _) =>
        {
            if (DataContext is OrthoSpineAI.UI.ViewModels.LoginViewModel vm)
                vm.Password = PasswordBox.Password;
        };
    }
}
