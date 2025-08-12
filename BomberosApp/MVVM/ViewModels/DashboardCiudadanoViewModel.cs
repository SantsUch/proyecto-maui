using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Views;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class DashboardCiudadanoViewModel
    {
        private readonly INavigation _navigation;

        public UsuarioModel UsuarioActual { get; set; }

        public ICommand ReportarIncidenteCommand { get; set; }
        public ICommand VerHistorialCommand { get; set; }
        public ICommand VerPerfilCommand { get; set; }
        public ICommand CerrarSesionCommand { get; set; }

        public DashboardCiudadanoViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            UsuarioActual = usuario ?? new UsuarioModel { Nombre = "Usuario" };

            ReportarIncidenteCommand = new Command(async () => await ReportarIncidente());
            VerHistorialCommand = new Command(async () => await VerHistorial());
            VerPerfilCommand = new Command(async () => await VerPerfil());
            CerrarSesionCommand = new Command(async () => await CerrarSesion());
        }

        private async Task ReportarIncidente()
        {
            // Pasar el usuario al ReportarIncidenteView para que pueda guardarse con el reporte
            await _navigation.PushAsync(new ReportarIncidenteView(UsuarioActual));
        }

        private async Task VerHistorial()
        {
            await _navigation.PushAsync(new HistorialReportesView(UsuarioActual));
        }

        private async Task VerPerfil()
        {
            await _navigation.PushAsync(new PerfilUsuarioView(UsuarioActual));
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