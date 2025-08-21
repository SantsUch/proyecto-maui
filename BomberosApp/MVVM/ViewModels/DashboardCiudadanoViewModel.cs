using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Views;
using BomberosApp.MVVM.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class DashboardCiudadanoViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _navigation;
        private readonly IncidentesRepository _incidentesRepository;

        private int _totalIncidentes = 0;
        private int _incidentesActivos = 0;

        public UsuarioModel UsuarioActual { get; set; }

        // Propiedades para el dashboard con PropertyChanged
        public int TotalIncidentes
        {
            get => _totalIncidentes;
            set
            {
                _totalIncidentes = value;
                OnPropertyChanged(nameof(TotalIncidentes));
            }
        }

        public int IncidentesActivos
        {
            get => _incidentesActivos;
            set
            {
                _incidentesActivos = value;
                OnPropertyChanged(nameof(IncidentesActivos));
            }
        }

        public ObservableCollection<IncidenteModel> IncidentesRecientes { get; set; }

        public ICommand ReportarIncidenteCommand { get; set; }
        public ICommand VerHistorialCommand { get; set; }
        public ICommand VerPerfilCommand { get; set; }
        public ICommand CerrarSesionCommand { get; set; }
        public ICommand VerDetalleCommand { get; set; }

        public DashboardCiudadanoViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            _incidentesRepository = new IncidentesRepository();
            UsuarioActual = usuario ?? new UsuarioModel { Nombre = "Usuario" };

            IncidentesRecientes = new ObservableCollection<IncidenteModel>();

            InitializeCommands();
            _ = CargarIncidentesRecientes(); // Llamada asíncrona
        }

        private void InitializeCommands()
        {
            ReportarIncidenteCommand = new Command(async () => await ReportarIncidente());
            VerHistorialCommand = new Command(async () => await VerHistorial());
            VerPerfilCommand = new Command(async () => await VerPerfil());
            CerrarSesionCommand = new Command(async () => await CerrarSesion());
            VerDetalleCommand = new Command<IncidenteModel>(async (incidente) => await VerDetalle(incidente));
        }

        private async Task ReportarIncidente()
        {
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

        private async Task VerDetalle(IncidenteModel incidente)
        {
            if (incidente != null)
            {
                await Application.Current.MainPage.DisplayAlert("Detalle",
                    $"Título: {incidente.Titulo}\nFecha: {incidente.FechaReportado:dd/MM/yyyy}\nEstado: {incidente.Estado}",
                    "OK");
            }
        }

        private async Task CargarIncidentesRecientes()
        {
            try
            {
                IncidentesRecientes.Clear();

                // Inicializar valores en 0 por defecto
                TotalIncidentes = 0;
                IncidentesActivos = 0;

                if (string.IsNullOrEmpty(UsuarioActual.Id))
                {
                    Console.WriteLine("Usuario.Id está vacío - manteniendo valores en 0");
                    return;
                }

                Console.WriteLine($"Cargando incidentes para usuario: {UsuarioActual.Id}");
                var incidentesUsuario = await _incidentesRepository.ObtenerPorUsuarioAsync(UsuarioActual.Id);

                Console.WriteLine($"Incidentes encontrados en Firebase: {incidentesUsuario.Count}");

                if (incidentesUsuario.Count == 0)
                {
                    Console.WriteLine("No hay incidentes en Firebase - manteniendo valores en 0");
                    return;
                }

                // Calcular estadísticas
                TotalIncidentes = incidentesUsuario.Count;

                // Estados que se consideran "En Proceso"
                var estadosEnProceso = new[] { "Reportado", "En Revisión", "Asignado", "En Proceso", "En Camino" };
                IncidentesActivos = incidentesUsuario.Count(i =>
                    !string.IsNullOrEmpty(i.Estado) &&
                    estadosEnProceso.Contains(i.Estado));

                Console.WriteLine($"Total: {TotalIncidentes}, En Proceso: {IncidentesActivos}");

                // Tomar solo los 3 más recientes para mostrar
                var recientes = incidentesUsuario.Take(3).ToList();

                foreach (var incidente in recientes)
                {
                    IncidentesRecientes.Add(incidente);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar incidentes: {ex.Message}");
                // En caso de error, mantener los valores en 0
                TotalIncidentes = 0;
                IncidentesActivos = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}