using BomberosApp.MVVM.ViewModels;

namespace BomberosApp.MVVM.Views;

public partial class GestionUsuariosView : ContentPage
{
    public GestionUsuariosView()
    {
        InitializeComponent();
        BindingContext = new GestionUsuariosViewModel(Navigation);
    }
}