using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class ReportarIncidenteViewModel
    {
        private readonly IncidentesRepository _repository;
        private UsuarioModel _usuarioActual;

        public IncidenteModel IncidenteTO { get; set; } = new IncidenteModel();

        public ICommand ReportarCommand { get; set; }
        public ICommand PickImageCommand { get; set; }
        public ICommand CancelarCommand { get; set; }

        public INavigation _navigation { get; set; }

        // Constructor original
        public ReportarIncidenteViewModel(INavigation navigation)
        {
            _repository = new IncidentesRepository();

            PickImageCommand = new Command(async () => await PickImage());
            ReportarCommand = new Command(async () => await ReportarIncidente());
            CancelarCommand = new Command(async () => await Cancelar());

            _navigation = navigation;
        }

        // Nuevo constructor que recibe el usuario
        public ReportarIncidenteViewModel(INavigation navigation, UsuarioModel usuario) : this(navigation)
        {
            _usuarioActual = usuario;
        }

        private async Task ReportarIncidente()
        {
            try
            {
                if (Validar())
                {
                    // Mostrar mensaje de confirmación
                    bool confirmar = await Application.Current.MainPage.DisplayAlert(
                        "Confirmación",
                        "¿Está seguro de que desea enviar el reporte?",
                        "Sí",
                        "No"
                    );

                    if (!confirmar)
                        return; // Canceló el usuario

                    // IMPORTANTE: Asignar el usuario al incidente
                    if (_usuarioActual != null)
                    {
                        IncidenteTO.UsuarioId = _usuarioActual.Id;
                        IncidenteTO.UsuarioNombre = _usuarioActual.Nombre;
                    }

                    // Establecer estado inicial
                    IncidenteTO.Estado = "Reportado";

                    // Proceder con el guardado si confirmó
                    await _repository.CrearIncidenteAsync(IncidenteTO);
                    await ShowMessage("Incidente reportado exitosamente!", true);
                    await _navigation.PopAsync();
                    LimpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al reportar el incidente: {ex.Message}");
                await ShowMessage("Error al enviar el reporte. Intente nuevamente.", false);
            }
        }

        private async Task PickImage()
        {
            // Sin Implementar
            await ShowMessage("Funcionalidad de imagen en desarrollo", false);
        }

        private async Task Cancelar()
        {
            bool confirmado = await App.Current.MainPage.DisplayAlert(
                "Cancelar",
                "¿Desea cancelar el reporte de incidente y volver al inicio?",
                "Sí",
                "No");

            if (confirmado)
            {
                await _navigation.PopAsync();
            }
        }

        private bool Validar()
        {
            bool valido = true;

            if (string.IsNullOrWhiteSpace(IncidenteTO.Titulo) ||
                string.IsNullOrWhiteSpace(IncidenteTO.Descripcion) ||
                string.IsNullOrWhiteSpace(IncidenteTO.Ubicacion))
            {
                ShowMessage("Por favor complete todos los campos obligatorios", false);
                valido = false;
            }

            return valido;
        }

        private void LimpiarFormulario()
        {
            IncidenteTO = new IncidenteModel();
        }

        private async Task ShowMessage(string mensaje, bool esExito)
        {
            await App.Current.MainPage.DisplayAlert(
                esExito ? "Éxito" : "Error",
                mensaje,
                "OK");
        }
    }
}