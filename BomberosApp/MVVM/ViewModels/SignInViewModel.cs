using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class SignInViewModel
    {
        private readonly UsuariosRepository _repository;
        private readonly INavigation _navigation;

        public UsuarioModel Usuario { get; set; } = new UsuarioModel();

        public string ConfirmarContrasena { get; set; }

        public ICommand RegistrarCommand { get; set; }

        public ICommand CancelarCommand { get; set; }

        public SignInViewModel(INavigation navigation)
        {
            _repository = new UsuariosRepository();
            _navigation = navigation;

            CancelarCommand = new Command(async () => await Cancelar());
            RegistrarCommand = new Command(async () => await Registrar());
        }

        private async Task Registrar()
        {
            if (!ValidarCampos())
                return;

            bool correoExiste = await _repository.EmailExistsAsync(Usuario.Correo);
            if (correoExiste)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Este correo ya está registrado.", "OK");
                return;
            }

            Usuario.Rol = "Civil";

            await _repository.CreateDocumentAsync(Usuario);

            await App.Current.MainPage.DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");

            LimpiarFormulario();

            // Opcional: Navegar al login después del registro
            await _navigation.PopAsync(); // o PushAsync(new LoginPage()) si quieres ir hacia adelante
        }

        private bool ValidarCampos()
        {
            // Validar campos vacíos primero
            if (string.IsNullOrWhiteSpace(Usuario.Nombre) ||
                string.IsNullOrWhiteSpace(Usuario.Correo) ||
                string.IsNullOrWhiteSpace(Usuario.Contrasena) ||
                string.IsNullOrWhiteSpace(ConfirmarContrasena))
            {
                App.Current.MainPage.DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                return false;
            }

            // Validar formato de correo
            if (!Regex.IsMatch(Usuario.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                App.Current.MainPage.DisplayAlert("Error", "Formato de correo inválido.", "OK");
                return false;
            }

            // Validar longitud de la contraseña
            if (Usuario.Contrasena.Length < 6)
            {
                App.Current.MainPage.DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres.", "OK");
                return false;
            }

            // Validar coincidencia de contraseñas
            if (Usuario.Contrasena != ConfirmarContrasena)
            {
                App.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
                return false;
            }

            return true;
        }
        private async Task Cancelar()
        {
            bool confirmado = await App.Current.MainPage.DisplayAlert(
                "Volver al inicio de sesión",
                "¿Deseas volver al inicio de sesión?",
                "Sí",
                "No");

            if (confirmado)
            {
                _navigation.PopAsync();
            }
        }

        private void LimpiarFormulario()
        {
            Usuario = new UsuarioModel();
        }

        private async Task ShowMessage(string mensaje, bool esCorto)
        {
            await App.Current.MainPage.DisplayAlert("Mensaje", mensaje, "OK");
        }
    }
}
