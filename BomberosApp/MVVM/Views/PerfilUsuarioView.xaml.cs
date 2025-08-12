using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class PerfilUsuarioView : ContentPage
{
    public PerfilUsuarioView(UsuarioModel usuario)
    {
        InitializeComponent();
        BindingContext = new PerfilUsuarioViewModel(Navigation, usuario);
    }
}