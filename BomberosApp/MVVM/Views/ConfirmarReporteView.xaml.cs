using BomberosApp.MVVM.ViewModels;

namespace BomberosApp.MVVM.Views;
public partial class ConfirmarReporteView : ContentPage
{
	public ConfirmarReporteView()
	{
		InitializeComponent();
		BindingContext = new ConfirmarReporteViewModel();
    }
}