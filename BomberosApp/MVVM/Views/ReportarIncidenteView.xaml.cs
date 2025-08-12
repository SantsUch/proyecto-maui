using BomberosApp.MVVM.ViewModels;

namespace BomberosApp.MVVM.Views;
public partial class ReportarIncidenteView : ContentPage
{
	public ReportarIncidenteView()
	{
		InitializeComponent();
        BindingContext = new ReportarIncidenteViewModel(Navigation);
    }
}