using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;
public partial class ReportarIncidenteView : ContentPage
{
    public ReportarIncidenteView()
    {
        InitializeComponent();
        BindingContext = new ReportarIncidenteViewModel(Navigation);
    }

    // Nuevo constructor que recibe el usuario
    public ReportarIncidenteView(UsuarioModel usuario)
    {
        InitializeComponent();
        BindingContext = new ReportarIncidenteViewModel(Navigation, usuario);
    }
}