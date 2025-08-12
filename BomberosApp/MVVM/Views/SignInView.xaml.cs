using BomberosApp.MVVM.ViewModels;

namespace BomberosApp.MVVM.Views;

public partial class SignInView : ContentPage
{
	public SignInView()
	{
		InitializeComponent();
        BindingContext = new SignInViewModel(Navigation);
    }
}