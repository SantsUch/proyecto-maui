using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class DetalleIncidenteViewModel
    {
        private readonly INavigation _navigation;
        private readonly IncidentesRepository _incidentesRepository;

        public IncidenteModel Incidente { get; set; }
        public UsuarioModel Funcionario { get; set; }
        public bool EsFinalizable { get; set; }
        public bool IsActualizando { get; set; }

        public ObservableCollection<string> EstadosDisponibles { get; set; } = new()
        {
            IncidenteModel.Estados.Asignado,
            IncidenteModel.Estados.EnProceso,
            IncidenteModel.Estados.Resuelto
        };

        public string NuevoEstado { get; set; }

        public ICommand CambiarEstadoCommand { get; }
        public ICommand FinalizarCommand { get; }

        public DetalleIncidenteViewModel(INavigation navigation, IncidenteModel incidente, UsuarioModel funcionario)
        {
            _navigation = navigation;
            _incidentesRepository = new IncidentesRepository();

            Incidente = incidente;
            Funcionario = funcionario;
            NuevoEstado = Incidente.Estado;

            EsFinalizable = Incidente?.Estado == IncidenteModel.Estados.Resuelto;

            CambiarEstadoCommand = new Command(async () => await CambiarEstado(), () => !IsActualizando);
            FinalizarCommand = new Command(async () => await Finalizar(), () => EsFinalizable && !IsActualizando);
        }

        private async Task CambiarEstado()
        {
            if (string.IsNullOrWhiteSpace(NuevoEstado) || NuevoEstado == Incidente.Estado) return;

            if (IsActualizando) return; // protección extra

            try
            {
                IsActualizando = true;
                ((Command)CambiarEstadoCommand).ChangeCanExecute();

                await _incidentesRepository.CambiarEstadoIncidenteAsync(Incidente.Id, NuevoEstado);
                Incidente.Estado = NuevoEstado;
                EsFinalizable = Incidente.Estado == IncidenteModel.Estados.Resuelto;

                await Application.Current.MainPage.DisplayAlert("Éxito", "Estado actualizado.", "OK");
            }
            finally
            {
                IsActualizando = false;
                ((Command)CambiarEstadoCommand).ChangeCanExecute();
            }
        }

        private void UpdateCanExecutes()
        {
            ((Command)CambiarEstadoCommand).ChangeCanExecute();
            ((Command)FinalizarCommand).ChangeCanExecute();
        }

        // Si usas Fody, puedes aprovechar los hooks automáticos:
        void OnIsActualizandoChanged() => UpdateCanExecutes();
        void OnEsFinalizableChanged() => UpdateCanExecutes();


        private async Task Finalizar()
        {
            if (Incidente.Estado != IncidenteModel.Estados.Resuelto)
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Solo pueden finalizarse los reportes resueltos.", "OK");
                return;
            }

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Finalizar incidente",
                "Una vez finalizado, el estado no podrá modificarse. ¿Desea continuar?",
                "Finalizar", "Cancelar");

            if (!confirmar) return;

            // Aquí puedes agregar lógica extra de cierre si la necesitas
            await Application.Current.MainPage.DisplayAlert("Completado", "Incidente finalizado.", "OK");
            await _navigation.PopAsync();
        }
    }
}
