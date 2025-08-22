using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views
{
    public partial class DashboardAdministradorView : ContentPage
    {
        public DashboardAdministradorView(UsuarioModel usuario)
        {
            InitializeComponent();
            BindingContext = new DashboardAdministradorViewModel(Navigation, usuario);
        }
    }
}