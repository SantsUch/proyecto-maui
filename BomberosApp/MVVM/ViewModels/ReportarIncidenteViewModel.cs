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
        public ICommand SeleccionarImagenCommand { get; set; }
        public ICommand CancelarCommand { get; set; }
        public ICommand ObtenerUbicacionCommand { get; set; }

        public INavigation _navigation { get; set; }

        private bool _isReporting = false;

        // Constructor original
        public ReportarIncidenteViewModel(INavigation navigation)
        {
            _repository = new IncidentesRepository();

            SeleccionarImagenCommand = new Command(async () => await SeleccionarImagen());
            ReportarCommand = new Command(async () => await ReportarIncidente());
            CancelarCommand = new Command(async () => await Cancelar());
            ObtenerUbicacionCommand = new Command(async () => await ObtenerUbicacion());

            _navigation = navigation;
        }

        // Nuevo constructor que recibe el usuario
        public ReportarIncidenteViewModel(INavigation navigation, UsuarioModel usuario) : this(navigation)
        {
            _usuarioActual = usuario;
        }

        private async Task ReportarIncidente()
        {
            // Evita dobles envíos por taps rápidos
            if (_isReporting) return;

            
            if (!Validar())
                return;

            if (_usuarioActual != null)
            {
                IncidenteTO.UsuarioId = _usuarioActual.Id;
                IncidenteTO.UsuarioNombre = _usuarioActual.Nombre;
            }


            // Mostrar confirmación con ubicación visible (si no hay, avisa)
            var ubicacion = string.IsNullOrWhiteSpace(IncidenteTO.Ubicacion)
                ? "(sin ubicación especificada)"
                : IncidenteTO.Ubicacion.Trim();

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar reporte",
                $"¿Desea enviar el incidente con la siguiente ubicación?\n\n{ubicacion}",
                "Sí, enviar",
                "Cancelar"
            );

            if (!confirmar)
                return;

            // Estado inicial + marca de tiempo si tu modelo la soporta
            IncidenteTO.Estado = "Reportado";
            IncidenteTO.FechaReportado = DateTime.UtcNow;

            try
            {
                _isReporting = true;

                await _repository.CrearIncidenteAsync(IncidenteTO);

                await ShowMessage("¡Incidente reportado exitosamente!", true);

                // Si esta página se cierra tras reportar, limpiar es opcional; mantenlo si reutilizas la vista
                LimpiarFormulario();

                // Asegura el await (tu v1 lo tenía bien, la v2 lo omitió)
                await _navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al reportar el incidente: {ex}");
                await ShowMessage("Error al enviar el reporte. Intente nuevamente.", false);
            }
            finally
            {
                _isReporting = false;
            }
        }


        private async Task SeleccionarImagen()
        {
            try
            {
                string opcion = await Application.Current.MainPage.DisplayActionSheet(
                    "Seleccionar imagen",
                    "Cancelar",
                    null,
                    "Tomar foto",
                    "Elegir de galería");

                FileResult photo = null;

                if (opcion == "Tomar foto")
                {
                    photo = await MediaPicker.CapturePhotoAsync();
                }
                else if (opcion == "Elegir de galería")
                {
                    photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                    {
                        Title = "Seleccione una imagen para el reporte"
                    });
                }

                if (photo != null)
                {
                    var ruta = photo.FullPath;

                    // Guardar en el modelo del incidente
                    IncidenteTO.Imagen = ruta;

                    await ShowMessage("Imagen seleccionada correctamente.", true);
                }
            }
            catch (FeatureNotSupportedException)
            {
                await ShowMessage("La función no es soportada en este dispositivo.", false);
            }
            catch (PermissionException)
            {
                await ShowMessage("Permiso denegado para acceder a la cámara o imágenes.", false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al seleccionar imagen: {ex.Message}");
            }
        }

        private async Task ObtenerUbicacion()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    // Guardar lat/lon
                    IncidenteTO.Latitud = location.Latitude;
                    IncidenteTO.Longitud = location.Longitude;

                    // Obtener dirección legible
                    var placemarks = await Geocoding.GetPlacemarksAsync(location);
                    var placemark = placemarks?.FirstOrDefault();

                    if (placemark != null)
                    {
                        IncidenteTO.Ubicacion = $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, " +
                                                 $"{placemark.Locality}, {placemark.AdminArea}, {placemark.CountryName}";
                    }

                    await ShowMessage($"Ubicación: {IncidenteTO.Ubicacion}", true);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine($"GPS no soportado: {fnsEx.Message}");
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine($"Permiso de ubicación denegado: {pEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la ubicación: {ex.Message}");
            }
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