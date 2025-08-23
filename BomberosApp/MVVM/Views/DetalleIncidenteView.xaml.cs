using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class DetalleIncidenteView : ContentPage
{
    public DetalleIncidenteView(IncidenteModel incidente, UsuarioModel funcionario)
    {
        InitializeComponent();
        BindingContext = new DetalleIncidenteViewModel(Navigation, incidente, funcionario);
    }
}