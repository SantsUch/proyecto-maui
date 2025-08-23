using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views;

public partial class DashboardFuncionarioView : ContentPage
{
    public DashboardFuncionarioView(UsuarioModel funcionario)
    {
        InitializeComponent();
        BindingContext = new DashBoardFuncionarioViewModel(Navigation, funcionario);
    }
}