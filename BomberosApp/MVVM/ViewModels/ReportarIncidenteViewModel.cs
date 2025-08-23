using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BomberosApp.MVVM.ViewModels
{
    public class ReportarIncidenteViewModel : INotifyPropertyChanged
    {
        private readonly IncidentesRepository _repository;
        private UsuarioModel _usuarioActual;

        private IncidenteModel _incidenteTO = new();
        public IncidenteModel IncidenteTO
        {
            get => _incidenteTO;
            set { _incidenteTO = value; OnPropertyChanged(); }
        }

        public ICommand ReportarCommand { get; }
        public ICommand SeleccionarImagenCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand ObtenerUbicacionCommand { get; }

        public INavigation _navigation { get; }

        private bool _isReporting;
        public bool IsReporting
        {
            get => _isReporting;
            set
            {
                if (_isReporting == value) return;
                _isReporting = value;
                OnPropertyChanged();
                // Si usas CanExecute en los botones:
                (ReportarCommand as Command)?.ChangeCanExecute();
                (SeleccionarImagenCommand as Command)?.ChangeCanExecute();
                (ObtenerUbicacionCommand as Command)?.ChangeCanExecute();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ReportarIncidenteViewModel(INavigation navigation)
        {
            _repository = new IncidentesRepository();
            _navigation = navigation;

            // Deshabilitamos comandos mientras IsReporting == true
            ReportarCommand = new Command(async () => await ReportarIncidente(), () => !IsReporting);
            SeleccionarImagenCommand = new Command(async () => await SeleccionarImagen(), () => !IsReporting);
            CancelarCommand = new Command(async () => await Cancelar());
            ObtenerUbicacionCommand = new Command(async () => await ObtenerUbicacion(), () => !IsReporting);
        }

        public ReportarIncidenteViewModel(INavigation navigation, UsuarioModel usuario) : this(navigation)
        {
            _usuarioActual = usuario;
        }

        private async Task ReportarIncidente()
        {
            // Evita dobles taps
            if (IsReporting) return;

            // Validar antes de continuar
            if (!Validar()) return;

            // (UX) Pedimos confirmación ANTES de activar el spinner
            var ubicacion = string.IsNullOrWhiteSpace(IncidenteTO.Ubicacion)
                ? "(sin ubicación especificada)"
                : IncidenteTO.Ubicacion.Trim();

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar reporte",
                $"¿Desea enviar el incidente con la siguiente ubicación?\n\n{ubicacion}",
                "Sí, enviar",
                "Cancelar"
            );
            if (!confirmar) return;

            try
            {
                IsReporting = true; // 🔹 Activa overlay

                // Asignar usuario si existe (opcional para reportes anónimos)
                if (_usuarioActual != null)
                {
                    IncidenteTO.UsuarioId = _usuarioActual.Id;
                    IncidenteTO.UsuarioNombre = _usuarioActual.Nombre;
                }

                // Estado + fecha
                IncidenteTO.Estado = "Reportado";
                IncidenteTO.FechaReportado = DateTime.UtcNow;

                // Guardar
                await _repository.CrearIncidenteAsync(IncidenteTO);

                await ShowMessage("¡Incidente reportado exitosamente!", true);

                LimpiarFormulario();

                await _navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al reportar el incidente: {ex}");
                await ShowMessage("Error al enviar el reporte. Intente nuevamente.", false);
            }
            finally
            {
                IsReporting = false; // 🔹 Apaga overlay SIEMPRE
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
                    using var stream = await photo.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);

                    // Convertir a Base64 y guardar en el modelo
                    IncidenteTO.ImagenBase64 = Convert.ToBase64String(ms.ToArray());

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