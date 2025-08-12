using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class SeguimientoIncidenteViewModel
    {
        private readonly IncidentesRepository _incidentesRepository;
        private readonly INavigation _navigation;

        public IncidenteModel Incidente { get; set; }
        public UsuarioModel FuncionarioAsignado { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public ObservableCollection<EstadoHistorial> HistorialEstados { get; set; }

        public ICommand ActualizarEstadoCommand { get; set; }
        public ICommand ContactarSoporteCommand { get; set; }
        public ICommand VolverCommand { get; set; }

        public SeguimientoIncidenteViewModel(INavigation navigation, IncidenteModel incidente)
        {
            _navigation = navigation;
            _incidentesRepository = new IncidentesRepository();
            Incidente = incidente ?? new IncidenteModel();

            HistorialEstados = new ObservableCollection<EstadoHistorial>();

            InitializeCommands();
            CargarDatos();
        }

        private void InitializeCommands()
        {
            ActualizarEstadoCommand = new Command(async () => await ActualizarEstado());
            ContactarSoporteCommand = new Command(async () => await ContactarSoporte());
            VolverCommand = new Command(async () => await _navigation.PopAsync());
        }

        private void CargarDatos()
        {
            UltimaActualizacion = DateTime.Now;

            // Crear historial de ejemplo
            HistorialEstados.Clear();

            var historial = new List<EstadoHistorial>
            {
                new EstadoHistorial
                {
                    Estado = "Reportado",
                    Fecha = Incidente.FechaReportado
                },
                new EstadoHistorial
                {
                    Estado = "En Revisión",
                    Fecha = Incidente.FechaReportado.AddMinutes(30)
                }
            };

            if (!string.IsNullOrEmpty(Incidente.Estado) && Incidente.Estado != "Reportado")
            {
                historial.Add(new EstadoHistorial
                {
                    Estado = Incidente.Estado,
                    Fecha = Incidente.FechaAsignacion ?? DateTime.Now
                });
            }

            foreach (var estado in historial.OrderByDescending(h => h.Fecha))
            {
                HistorialEstados.Add(estado);
            }

            // Cargar funcionario asignado si existe
            if (!string.IsNullOrEmpty(Incidente.FuncionarioAsignadoNombre))
            {
                FuncionarioAsignado = new UsuarioModel
                {
                    Nombre = Incidente.FuncionarioAsignadoNombre,
                    Correo = "funcionario@bomberos.go.cr" // Ejemplo
                };
            }
        }

        private async Task ActualizarEstado()
        {
            try
            {
                // Simular actualización
                await Application.Current.MainPage.DisplayAlert("Actualizar",
                    "Estado actualizado correctamente", "OK");

                UltimaActualizacion = DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar estado: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudo actualizar el estado", "OK");
            }
        }

        private async Task ContactarSoporte()
        {
            string[] opciones = { "Llamar", "Enviar Email", "Cancelar" };

            string resultado = await Application.Current.MainPage.DisplayActionSheet(
                "Contactar Soporte",
                "Cancelar",
                null,
                opciones);

            switch (resultado)
            {
                case "Llamar":
                    await Application.Current.MainPage.DisplayAlert("Llamar",
                        "Marcando: 911", "OK");
                    break;
                case "Enviar Email":
                    await Application.Current.MainPage.DisplayAlert("Email",
                        "Abriendo aplicación de correo...", "OK");
                    break;
            }
        }
    }

    // Clase auxiliar para el historial de estados
    public class EstadoHistorial
    {
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}