using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class SeguimientoIncidenteView : ContentPage
{
    public SeguimientoIncidenteView(IncidenteModel incidente)
    {
        InitializeComponent();
        BindingContext = new SeguimientoIncidenteViewModel(Navigation, incidente);
    }
}