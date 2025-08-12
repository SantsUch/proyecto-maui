using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class ReportarIncidenteViewModel
    {
        private readonly IncidentesRepository _repository;

        public IncidenteModel IncidenteTO { get; set; } = new IncidenteModel();

        public ICommand ReportarCommand { get; set; }
        public ICommand PickImageCommand { get; set; }
        public ICommand CancelarCommand { get; set; }

        public INavigation _navigation { get; set; }

        public ReportarIncidenteViewModel(INavigation navigation)
        {
            _repository = new IncidentesRepository();

            PickImageCommand = new Command(async () => await PickImage());
            ReportarCommand = new Command(async () => await ReportarIncidente());
            CancelarCommand = new Command(async () => await Cancelar());

            _navigation = navigation;
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
            }
        }

        private async Task PickImage()
        {
            // Sin Implementar
        }

        private async Task Cancelar()
        {
            bool confirmado = await App.Current.MainPage.DisplayAlert(
                "Cancelar",
                "¿Desea cancelar el reporte de incidente y volder al inicio?",
                "Sí",
                "No");

            if (confirmado)
            {
                _navigation.PopAsync();
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

        private async Task ShowMessage(string mensaje, bool esCorto)
        {
            await App.Current.MainPage.DisplayAlert("Mensaje", mensaje, "OK");
        }
    }
}
