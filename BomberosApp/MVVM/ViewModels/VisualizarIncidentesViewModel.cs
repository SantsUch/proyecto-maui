using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class VisualizarIncidentesViewModel : INotifyPropertyChanged
    {
        private readonly IncidentesRepository _incidentesRepository;
        private readonly INavigation _navigation;
        private UsuarioModel _usuarioAdministrador;

        public ObservableCollection<IncidenteModel> TodosLosIncidentes { get; set; }
        public ObservableCollection<string> EstadosFiltro { get; set; }

        public IncidenteModel IncidenteSeleccionado { get; set; }
        public string EstadoFiltroSeleccionado { get; set; }

        // Propiedades para mostrar detalles del incidente seleccionado
        public bool MostrarDetallesIncidente => IncidenteSeleccionado != null;
        public string TituloIncidenteSeleccionado => IncidenteSeleccionado?.Titulo ?? "";
        public string DescripcionIncidenteSeleccionado => IncidenteSeleccionado?.Descripcion ?? "";
        public string UbicacionIncidenteSeleccionado => IncidenteSeleccionado?.Ubicacion ?? "";
        public string FechaIncidenteSeleccionado => IncidenteSeleccionado?.FechaReportado.ToString("dd/MM/yyyy HH:mm") ?? "";
        public string EstadoIncidenteSeleccionado => IncidenteSeleccionado?.Estado ?? "Reportado";
        public string FuncionarioAsignado => IncidenteSeleccionado?.FuncionarioAsignadoNombre ?? "Sin asignar";
        public string CategoriaIncidente => IncidenteSeleccionado?.Categoria ?? "Sin categoría";
        public string PrioridadIncidente => IncidenteSeleccionado?.Prioridad ?? "Sin prioridad";
        public bool MostrarImagenIncidente => !string.IsNullOrEmpty(IncidenteSeleccionado?.ImagenBase64);
        public string ImagenIncidenteBase64 => IncidenteSeleccionado?.ImagenBase64 ?? "";

        public ICommand ActualizarListaCommand { get; set; }
        public ICommand SeleccionarIncidenteCommand { get; set; }
        public ICommand EliminarIncidenteCommand { get; set; }
        public ICommand CerrarDetallesCommand { get; set; }
        public ICommand VolverCommand { get; set; }

        public VisualizarIncidentesViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            _incidentesRepository = new IncidentesRepository();
            _usuarioAdministrador = usuario;

            InitializeCollections();
            InitializeCommands();
            CargarIncidentes();
        }

        private void InitializeCollections()
        {
            TodosLosIncidentes = new ObservableCollection<IncidenteModel>();

            EstadosFiltro = new ObservableCollection<string>
            {
                "Todos",
                "Reportado",
                "Asignado",
                "En Proceso",
                "Resuelto",
                "Cancelado"
            };

            EstadoFiltroSeleccionado = "Todos";
        }

        private void InitializeCommands()
        {
            ActualizarListaCommand = new Command(async () => await CargarIncidentes());
            SeleccionarIncidenteCommand = new Command<IncidenteModel>(SeleccionarIncidente);
            EliminarIncidenteCommand = new Command<IncidenteModel>(async (incidente) => await EliminarIncidente(incidente));
            CerrarDetallesCommand = new Command(CerrarDetalles);
            VolverCommand = new Command(async () => await _navigation.PopAsync());
        }

        private async Task CargarIncidentes()
        {
            try
            {
                var todosIncidentes = await _incidentesRepository.ObtenerTodosAsync();
                var incidentesFiltrados = todosIncidentes
                    .Where(i => FiltrarPorEstado(i.Object))
                    .OrderByDescending(i => i.Object.FechaReportado)
                    .Select(i =>
                    {
                        var incidente = i.Object;
                        incidente.Key = i.Key;
                        return incidente;
                    })
                    .ToList();

                TodosLosIncidentes.Clear();
                foreach (var incidente in incidentesFiltrados)
                {
                    TodosLosIncidentes.Add(incidente);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar incidentes: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudieron cargar los incidentes", "OK");
            }
        }

        private bool FiltrarPorEstado(IncidenteModel incidente)
        {
            return EstadoFiltroSeleccionado switch
            {
                "Todos" => true,
                _ => incidente.Estado == EstadoFiltroSeleccionado
            };
        }

        private void SeleccionarIncidente(IncidenteModel incidente)
        {
            IncidenteSeleccionado = incidente;

            // Notificar cambios para mostrar detalles
            OnPropertyChanged(nameof(IncidenteSeleccionado));
            OnPropertyChanged(nameof(MostrarDetallesIncidente));
            OnPropertyChanged(nameof(TituloIncidenteSeleccionado));
            OnPropertyChanged(nameof(DescripcionIncidenteSeleccionado));
            OnPropertyChanged(nameof(UbicacionIncidenteSeleccionado));
            OnPropertyChanged(nameof(FechaIncidenteSeleccionado));
            OnPropertyChanged(nameof(EstadoIncidenteSeleccionado));
            OnPropertyChanged(nameof(FuncionarioAsignado));
            OnPropertyChanged(nameof(CategoriaIncidente));
            OnPropertyChanged(nameof(PrioridadIncidente));
            OnPropertyChanged(nameof(MostrarImagenIncidente));
            OnPropertyChanged(nameof(ImagenIncidenteBase64));
        }

        private async Task EliminarIncidente(IncidenteModel incidente)
        {
            if (incidente == null) return;

            // Solo permitir eliminar incidentes resueltos
            if (incidente.Estado != IncidenteModel.Estados.Resuelto)
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    "Solo se pueden eliminar incidentes que estén resueltos.", "OK");
                return;
            }

            bool confirmacion = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Eliminación",
                $"¿Estás seguro de que deseas eliminar el incidente '{incidente.Titulo}'?\n\nEsta acción no se puede deshacer.",
                "Sí, Eliminar",
                "Cancelar");

            if (confirmacion)
            {
                try
                {
                    await _incidentesRepository.EliminarIncidenteAsync(incidente.Id);
                    TodosLosIncidentes.Remove(incidente);

                    // Si era el incidente seleccionado, limpiar detalles
                    if (IncidenteSeleccionado?.Id == incidente.Id)
                    {
                        CerrarDetalles();
                    }

                    await Application.Current.MainPage.DisplayAlert("Éxito",
                        "El incidente ha sido eliminado correctamente.", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        $"No se pudo eliminar el incidente: {ex.Message}", "OK");
                }
            }
        }

        private void CerrarDetalles()
        {
            IncidenteSeleccionado = null;
            OnPropertyChanged(nameof(IncidenteSeleccionado));
            OnPropertyChanged(nameof(MostrarDetallesIncidente));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}