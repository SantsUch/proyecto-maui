using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class HistorialReportesViewModel : INotifyPropertyChanged
    {
        private readonly IncidentesRepository _incidentesRepository;
        private readonly INavigation _navigation;

        public UsuarioModel Usuario { get; set; }
        public ObservableCollection<IncidenteModel> MisIncidentes { get; set; }

        public ICommand VerDetalleIncidenteCommand { get; set; }
        public ICommand VolverCommand { get; set; }
        public ICommand ActualizarCommand { get; set; }

        public HistorialReportesViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            Usuario = usuario ?? new UsuarioModel();
            _incidentesRepository = new IncidentesRepository();

            MisIncidentes = new ObservableCollection<IncidenteModel>();

            VerDetalleIncidenteCommand = new Command<IncidenteModel>(async (incidente) => await VerDetalle(incidente));
            VolverCommand = new Command(async () => await _navigation.PopAsync());
            ActualizarCommand = new Command(async () => await CargarIncidentes());

            _ = CargarIncidentes(); // Llamada asíncrona sin await para no bloquear constructor
        }

        private async Task CargarIncidentes()
        {
            try
            {
                MisIncidentes.Clear();

                if (string.IsNullOrEmpty(Usuario.Id))
                {
                    Console.WriteLine("Usuario.Id está vacío - no hay incidentes para mostrar");
                    OnPropertyChanged(nameof(MisIncidentes));
                    return;
                }

                Console.WriteLine($"Cargando incidentes para usuario: {Usuario.Id}");
                var incidentesUsuario = await _incidentesRepository.ObtenerPorUsuarioAsync(Usuario.Id);

                Console.WriteLine($"Incidentes encontrados en Firebase: {incidentesUsuario.Count}");

                if (incidentesUsuario.Count == 0)
                {
                    Console.WriteLine("No hay incidentes en Firebase para este usuario");
                    OnPropertyChanged(nameof(MisIncidentes));
                    return;
                }

                foreach (var incidente in incidentesUsuario)
                {
                    MisIncidentes.Add(incidente);
                }

                OnPropertyChanged(nameof(MisIncidentes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar incidentes desde Firebase: {ex.Message}");
                // En caso de error
                OnPropertyChanged(nameof(MisIncidentes));
            }
        }

        private async Task VerDetalle(IncidenteModel incidente)
        {
            if (incidente != null)
            {
                var detalles = $"Título: {incidente.Titulo}\n\n" +
                              $"Descripción: {incidente.Descripcion}\n\n" +
                              $"Ubicación: {incidente.Ubicacion}\n\n" +
                              $"Estado: {incidente.Estado}\n\n" +
                              $"Fecha: {incidente.FechaReportado:dd/MM/yyyy HH:mm}";

                if (!string.IsNullOrEmpty(incidente.FuncionarioAsignadoNombre))
                {
                    detalles += $"\n\nFuncionario: {incidente.FuncionarioAsignadoNombre}";
                }

                if (!string.IsNullOrEmpty(incidente.Categoria))
                {
                    detalles += $"\n\nCategoría: {incidente.Categoria}";
                }

                if (!string.IsNullOrEmpty(incidente.Prioridad))
                {
                    detalles += $"\n\nPrioridad: {incidente.Prioridad}";
                }

                await Application.Current.MainPage.DisplayAlert("Detalle del Incidente", detalles, "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}