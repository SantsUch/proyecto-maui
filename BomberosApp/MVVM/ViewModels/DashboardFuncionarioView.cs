using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Views;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class DashboardFuncionarioViewModel
    {
        private readonly INavigation _navigation;

        public UsuarioModel Usuario { get; set; }

        public ICommand VerIncidentesAsignadosCommand { get; set; }
        public ICommand ReportarIncidenteCommand { get; set; }
        public ICommand VerPerfilCommand { get; set; }
        public ICommand CerrarSesionCommand { get; set; }

        public DashboardFuncionarioViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            Usuario = usuario ?? new UsuarioModel { Nombre = "Funcionario" };

            VerIncidentesAsignadosCommand = new Command(async () => await VerIncidentesAsignados());
            ReportarIncidenteCommand = new Command(async () => await ReportarIncidente());
            VerPerfilCommand = new Command(async () => await VerPerfil());
            CerrarSesionCommand = new Command(async () => await CerrarSesion());
        }

        private async Task VerIncidentesAsignados()
        {
            await Application.Current.MainPage.DisplayAlert("Información", "Funcionalidad en desarrollo", "OK");
        }

        private async Task ReportarIncidente()
        {
            await _navigation.PushAsync(new ReportarIncidenteView());
        }

        private async Task VerPerfil()
        {
            await Application.Current.MainPage.DisplayAlert("Información", "Funcionalidad en desarrollo", "OK");
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