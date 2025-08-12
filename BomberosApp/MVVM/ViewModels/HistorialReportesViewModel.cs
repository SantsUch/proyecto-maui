using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class HistorialReportesViewModel
    {
        private readonly IncidentesRepository _incidentesRepository;
        private readonly INavigation _navigation;

        public UsuarioModel Usuario { get; set; }
        public ObservableCollection<IncidenteModel> MisIncidentes { get; set; }

        public ICommand VerDetalleIncidenteCommand { get; set; }
        public ICommand VolverCommand { get; set; }

        public HistorialReportesViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            Usuario = usuario ?? new UsuarioModel();
            _incidentesRepository = new IncidentesRepository();

            MisIncidentes = new ObservableCollection<IncidenteModel>();

            VerDetalleIncidenteCommand = new Command<IncidenteModel>(async (incidente) => await VerDetalle(incidente));
            VolverCommand = new Command(async () => await _navigation.PopAsync());

            CargarIncidentes();
        }

        private async Task CargarIncidentes()
        {
            try
            {
                // Por ahora mostrar algunos datos de ejemplo
                // Más adelante aquí cargarías los incidentes reales del usuario desde Firebase
                MisIncidentes.Clear();

                // Ejemplo temporal - puedes quitar esto cuando implementes Firebase
                var ejemplos = new List<IncidenteModel>
                {
                    new IncidenteModel
                    {
                        Titulo = "Incendio en casa",
                        FechaReportado = DateTime.Now.AddDays(-2),
                        Ubicacion = "San José Centro",
                        Estado = "En Proceso"
                    },
                    new IncidenteModel
                    {
                        Titulo = "Accidente de tránsito",
                        FechaReportado = DateTime.Now.AddDays(-5),
                        Ubicacion = "Cartago",
                        Estado = "Resuelto"
                    }
                };

                foreach (var incidente in ejemplos)
                {
                    MisIncidentes.Add(incidente);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar incidentes: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudieron cargar los reportes", "OK");
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
    }
}