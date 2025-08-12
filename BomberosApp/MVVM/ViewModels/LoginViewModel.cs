using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using BomberosApp.MVVM.Views;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    public class LoginViewModel
    {
        private readonly UsuariosRepository _repository;
        public UsuarioModel UsuarioTO { get; set; } = new UsuarioModel();

        public string Correo
        {
            get => UsuarioTO.Correo;
            set => UsuarioTO.Correo = value;
        }

        public string Contrasena
        {
            get => UsuarioTO.Contrasena;
            set => UsuarioTO.Contrasena = value;
        }

        public ICommand LoginCommand { get; set; }
        public ICommand CancelarCommand { get; set; }
        public ICommand ForgotPasswordCommand { get; set; }
        public ICommand GoToSignInCommand { get; set; }

        public INavigation _navigation { get; set; }

        public LoginViewModel(INavigation navigation)
        {
            _repository = new UsuariosRepository();
            _navigation = navigation;

            LoginCommand = new Command(async () => await Login());
            CancelarCommand = new Command(async () => await Cancelar());
            ForgotPasswordCommand = new Command(async () => await ForgotPassword());
            GoToSignInCommand = new Command(async () => await GoToSignIn());
        }

        private async Task Login()
        {
            try
            {
                var usuarios = await _repository.GetAllAsync();

                var usuarioEncontrado = usuarios
                    .FirstOrDefault(u =>
                        u.Value.Correo?.Trim().ToLower() == Correo?.Trim().ToLower() &&
                        u.Value.Contrasena == Contrasena);

                if (usuarioEncontrado.Value == null)
                {
                    await ShowMessage("Credenciales incorrectas", false);
                    return;
                }

                var usuario = usuarioEncontrado.Value;
                var rol = usuario.Rol;

                switch (rol)
                {
                    case "Civil":
                        await _navigation.PushAsync(new DashboardCiudadanoView(usuario));
                        break;
                    case "Funcionario":
                        await _navigation.PushAsync(new DashboardFuncionarioView(usuario)); // Por implementar
                        break;
                    case "Administrador":
                        await _navigation.PushAsync(new AsignarIncidentesView());
                        break;
                    default:
                        await ShowMessage("Rol no reconocido", false);
                        break;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar sesión: {ex}");
                await ShowMessage("Ocurrió un error durante el inicio de sesión", false);
            }
        }


        private async Task ForgotPassword()
        {
            await ShowMessage("Funcionalidad aún no implementada.", true);
        }

        private async Task GoToSignIn()
        {
            await _navigation.PushAsync(new SignInView());
        }

        private async Task Cancelar()
        {
            bool confirmado = await App.Current.MainPage.DisplayAlert(
                "Volver al inicio",
                "¿Deseas volver al inicio?",
                "Sí",
                "No");

            if (confirmado)
            {
                _navigation.PopAsync();
            }
        }

        private async Task ShowMessage(string message, bool success)
        {
            var toast = Toast.Make(message, ToastDuration.Short, 14);
            await toast.Show();
        }

    }
}

