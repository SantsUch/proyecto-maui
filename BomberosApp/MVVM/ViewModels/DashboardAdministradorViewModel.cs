using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Views;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class DashboardAdministradorViewModel
    {
        private readonly INavigation _navigation;

        public UsuarioModel Usuario { get; set; }

        public ICommand AsignarIncidentesCommand { get; set; }
        public ICommand GestionarUsuariosCommand { get; set; }
        public ICommand VerPerfilCommand { get; set; }
        public ICommand CerrarSesionCommand { get; set; }

        public DashboardAdministradorViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            Usuario = usuario ?? new UsuarioModel { Nombre = "Administrador" };

            AsignarIncidentesCommand = new Command(async () => await AsignarIncidentes());
            GestionarUsuariosCommand = new Command(async () => await GestionarUsuarios());
            VerPerfilCommand = new Command(async () => await VerPerfil());
            CerrarSesionCommand = new Command(async () => await CerrarSesion());
        }

        private async Task AsignarIncidentes()
        {
            await _navigation.PushAsync(new AsignarIncidentesView(Usuario));
        }

        private async Task GestionarUsuarios()
        {
            await _navigation.PushAsync(new GestionUsuariosView());
        }

        private async Task VerPerfil()
        {
            await _navigation.PushAsync(new PerfilUsuarioView(Usuario));
        }

        private async Task CerrarSesion()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión",
                "¿Está seguro que desea cerrar sesión?",
                "Sí",
                "No");

            if (confirmar)
            {
                await _navigation.PopToRootAsync();
            }
        }
    }
}