using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PropertyChanged;
using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using BomberosApp.MVVM.Views;

namespace BomberosApp.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class DashBoardFuncionarioViewModel
    {
        private readonly INavigation _navigation;
        private readonly IncidentesRepository _incidentesRepository;
        private readonly UsuarioModel _funcionario;

        public ObservableCollection<IncidenteModel> IncidentesAsignados { get; set; } = new();
        public IncidenteModel IncidenteSeleccionado { get; set; }
        public UsuarioModel Funcionario { get; set; } 

        public ObservableCollection<string> PrioridadesFiltro { get; set; } = new()
        {
            "Todos", "Crítica", "Alta", "Media", "Baja"
        };

        public string PrioridadSeleccionada { get; set; } = "Todos";

        public ICommand ActualizarListaCommand { get; }
        public ICommand SeleccionarIncidenteCommand { get; }

        public DashBoardFuncionarioViewModel(INavigation navigation, UsuarioModel funcionario)
        {
            _navigation = navigation;
            _funcionario = funcionario;
            _incidentesRepository = new IncidentesRepository();

            ActualizarListaCommand = new Command(async () => await CargarIncidentesAsignados());
            SeleccionarIncidenteCommand = new Command<IncidenteModel>(async (incidente) => await SeleccionarIncidente(incidente));

            _ = CargarIncidentesAsignados();
        }

        private async Task CargarIncidentesAsignados()
        {
            try
            {
                var snapshot = await _incidentesRepository.ObtenerTodosAsync();

                var lista = snapshot
                    .Select(s =>
                    {
                        var o = s.Object;
                        o.Key = s.Key; // guarda la key de Firebase
                        return o;
                    })
                    .Where(o => o.FuncionarioAsignadoId == _funcionario?.Id)
                    .OrderByDescending(o => o.FechaReportado)
                    .ToList();

                if (PrioridadSeleccionada != "Todos")
                    lista = lista.Where(o => o.Prioridad == PrioridadSeleccionada).ToList();

                IncidentesAsignados.Clear();
                foreach (var inc in lista)
                    IncidentesAsignados.Add(inc);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar incidentes asignados: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron cargar los incidentes asignados.", "OK");
            }
        }

        private async Task SeleccionarIncidente(IncidenteModel incidente)
        {
            if (incidente == null) return;

            await _navigation.PushAsync(new DetalleIncidenteView(incidente, _funcionario));
        }


        // Hook de Fody: se dispara cuando cambia PrioridadSeleccionada
        private void OnPrioridadSeleccionadaChanged() => _ = CargarIncidentesAsignados();
    }
}
