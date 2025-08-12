using BomberosApp.MVVM.ViewModels;

namespace BomberosApp.MVVM.Views;
public partial class LoginView : ContentPage
{
	public LoginView()
	{
		InitializeComponent();
        BindingContext = new LoginViewModel(this.Navigation);
    }
}