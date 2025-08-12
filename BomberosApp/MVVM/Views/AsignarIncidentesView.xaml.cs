using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class AsignarIncidentesView : ContentPage
{
    public AsignarIncidentesView()
    {
        InitializeComponent();
        BindingContext = new AsignarIncidentesViewModel(Navigation);
    }

    // Constructor que recibe el usuario administrador
    public AsignarIncidentesView(UsuarioModel usuario)
    {
        InitializeComponent();
        BindingContext = new AsignarIncidentesViewModel(Navigation, usuario);
    }
}