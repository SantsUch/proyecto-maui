using BomberosApp.MVVM.ViewModels;

namespace BomberosApp.MVVM.Views;
public partial class InicioView : ContentPage
{
	public InicioView()
	{
		InitializeComponent();
        BindingContext = new InicioViewModel(Navigation);
    }
}