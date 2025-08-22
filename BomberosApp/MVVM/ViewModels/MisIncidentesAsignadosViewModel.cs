using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class MisIncidentesAsignadosViewModel : INotifyPropertyChanged
    {
        private readonly IncidentesRepository _incidentesRepository;
        private readonly INavigation _navigation;
        private readonly UsuarioModel _funcionario;

        public ObservableCollection<IncidenteModel> MisIncidentes { get; set; }

        public ICommand ActualizarListaCommand { get; set; }
        public ICommand VolverCommand { get; set; }

        public MisIncidentesAsignadosViewModel(INavigation navigation, UsuarioModel funcionario)
        {
            _navigation = navigation;
            _funcionario = funcionario;
            _incidentesRepository = new IncidentesRepository();

            MisIncidentes = new ObservableCollection<IncidenteModel>();

            ActualizarListaCommand = new Command(async () => await CargarMisIncidentes());
            VolverCommand = new Command(async () => await _navigation.PopAsync());

            CargarMisIncidentes();
        }

        private async Task CargarMisIncidentes()
        {
            try
            {
                var todosIncidentes = await _incidentesRepository.ObtenerTodosAsync();

                // Filtrar solo los incidentes asignados a este funcionario
                var misIncidentes = todosIncidentes
                    .Where(i => i.Object.FuncionarioAsignadoId == _funcionario.Id)
                    .OrderByDescending(i => i.Object.FechaReportado)
                    .Select(i => i.Object)
                    .ToList();

                MisIncidentes.Clear();
                foreach (var incidente in misIncidentes)
                {
                    MisIncidentes.Add(incidente);
                }

                Console.WriteLine($"Incidentes cargados para {_funcionario.Nombre}: {MisIncidentes.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar mis incidentes: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudieron cargar los incidentes asignados", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}