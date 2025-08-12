using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class DashboardCiudadanoView : ContentPage
{
    public DashboardCiudadanoView(UsuarioModel usuario)
    {
        InitializeComponent();
        BindingContext = new DashboardCiudadanoViewModel(Navigation, usuario);
    }
}