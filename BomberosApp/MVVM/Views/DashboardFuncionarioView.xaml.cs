using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class DashboardFuncionarioView : ContentPage
{
    public DashboardFuncionarioView(UsuarioModel usuario)
    {
        InitializeComponent();
        BindingContext = new DashboardFuncionarioViewModel(Navigation, usuario);
    }
}