using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class PerfilUsuarioViewModel
    {
        private readonly UsuariosRepository _usuariosRepository;
        private readonly INavigation _navigation;

        public UsuarioModel Usuario { get; set; }

        // Campos para cambio de contraseña
        public string ContrasenaActual { get; set; } = string.Empty;
        public string NuevaContrasena { get; set; } = string.Empty;
        public string ConfirmarNuevaContrasena { get; set; } = string.Empty;

        // Estados de la UI
        public bool ModoLectura { get; set; } = true;
        public bool MostrarCambioContrasena { get; set; } = false;

        // Propiedades calculadas
        public string TextoBotonEdicion => ModoLectura ? "Editar Perfil" : "Cancelar";

        // Estadísticas simples
        public int TotalReportes { get; set; } = 0;
        public int ReportesActivos { get; set; } = 0;
        public int ReportesResueltos { get; set; } = 0;
        public DateTime FechaRegistro { get; set; } = DateTime.Now.AddMonths(-6); // Ejemplo

        // Comandos
        public ICommand ToggleEdicionCommand { get; set; }
        public ICommand GuardarCambiosPerfilCommand { get; set; }
        public ICommand MostrarCambioContrasenaCommand { get; set; }
        public ICommand GuardarCambiosContrasenaCommand { get; set; }
        public ICommand CancelarCambioContrasenaCommand { get; set; }
        public ICommand CerrarSesionCommand { get; set; }
        public ICommand VolverCommand { get; set; }

        public PerfilUsuarioViewModel(INavigation navigation, UsuarioModel usuario)
        {
            _navigation = navigation;
            _usuariosRepository = new UsuariosRepository();
            Usuario = usuario ?? new UsuarioModel();

            InitializeCommands();
            CargarEstadisticas();
        }

        private void InitializeCommands()
        {
            ToggleEdicionCommand = new Command(ToggleEdicion);
            GuardarCambiosPerfilCommand = new Command(async () => await GuardarCambiosPerfil());
            MostrarCambioContrasenaCommand = new Command(MostrarCambioContrasenaPanel);
            GuardarCambiosContrasenaCommand = new Command(async () => await GuardarCambiosContrasena());
            CancelarCambioContrasenaCommand = new Command(CancelarCambioContrasena);
            CerrarSesionCommand = new Command(async () => await CerrarSesion());
            VolverCommand = new Command(async () => await _navigation.PopAsync());
        }

        private void ToggleEdicion()
        {
            ModoLectura = !ModoLectura;

            if (ModoLectura)
            {
                // Si cancela la edición, restaurar datos originales si es necesario
                // Por ahora no hacemos nada
            }
        }

        private async Task GuardarCambiosPerfil()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Usuario.Nombre))
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "El nombre no puede estar vacío", "OK");
                    return;
                }

                // Aquí guardarías en Firebase
                // await _usuariosRepository.UpdateDocumentAsync(Usuario, Usuario.Key);

                await Application.Current.MainPage.DisplayAlert("Éxito",
                    "Perfil actualizado correctamente", "OK");

                ModoLectura = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar perfil: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudo actualizar el perfil", "OK");
            }
        }

        private void MostrarCambioContrasenaPanel()
        {
            MostrarCambioContrasena = true;
        }

        private async Task GuardarCambiosContrasena()
        {
            try
            {
                if (!ValidarCambioContrasena())
                    return;

                // Verificar contraseña actual
                if (Usuario.Contrasena != ContrasenaActual)
                {
                    await Application.Current.MainPage.DisplayAlert("Error",
                        "La contraseña actual es incorrecta", "OK");
                    return;
                }

                // Actualizar contraseña
                Usuario.Contrasena = NuevaContrasena;

                // Aquí guardarías en Firebase
                // await _usuariosRepository.UpdateDocumentAsync(Usuario, Usuario.Key);

                await Application.Current.MainPage.DisplayAlert("Éxito",
                    "Contraseña cambiada correctamente", "OK");

                CancelarCambioContrasena();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cambiar contraseña: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudo cambiar la contraseña", "OK");
            }
        }

        private bool ValidarCambioContrasena()
        {
            if (string.IsNullOrWhiteSpace(ContrasenaActual))
            {
                Application.Current.MainPage.DisplayAlert("Error",
                    "Debe ingresar la contraseña actual", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(NuevaContrasena))
            {
                Application.Current.MainPage.DisplayAlert("Error",
                    "Debe ingresar la nueva contraseña", "OK");
                return false;
            }

            if (NuevaContrasena.Length < 6)
            {
                Application.Current.MainPage.DisplayAlert("Error",
                    "La nueva contraseña debe tener al menos 6 caracteres", "OK");
                return false;
            }

            if (NuevaContrasena != ConfirmarNuevaContrasena)
            {
                Application.Current.MainPage.DisplayAlert("Error",
                    "Las contraseñas no coinciden", "OK");
                return false;
            }

            return true;
        }

        private void CancelarCambioContrasena()
        {
            MostrarCambioContrasena = false;
            ContrasenaActual = string.Empty;
            NuevaContrasena = string.Empty;
            ConfirmarNuevaContrasena = string.Empty;
        }

        private async Task CerrarSesion()
        {
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Cerrar Sesión",
                "¿Está seguro que desea cerrar sesión?",
                "Sí",
                "No");

            if (confirmar)
            {
                await _navigation.PopToRootAsync();
            }
        }

        private void CargarEstadisticas()
        {
            // Por ahora datos de ejemplo
            // Más adelante aquí cargarías las estadísticas reales desde Firebase
            TotalReportes = 5;
            ReportesActivos = 2;
            ReportesResueltos = 3;
        }
    }
}