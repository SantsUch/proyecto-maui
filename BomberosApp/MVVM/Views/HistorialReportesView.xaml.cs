using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class HistorialReportesView : ContentPage
{
    public HistorialReportesView(UsuarioModel usuario)
    {
        InitializeComponent();
        BindingContext = new HistorialReportesViewModel(Navigation, usuario);
    }
}