using BomberosApp.MVVM.Models;
using BomberosApp.MVVM.Repositories;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace BomberosApp.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class GestionUsuariosViewModel
    {
        private readonly UsuariosRepository _repository;
        private readonly INavigation _navigation;

        public ObservableCollection<UsuarioModelExtendido> TodosLosUsuarios { get; set; }
        public ObservableCollection<UsuarioModelExtendido> UsuariosFiltrados { get; set; }
        public ObservableCollection<string> RolesFiltro { get; set; }
        public ObservableCollection<string> RolesDisponibles { get; set; }

        public UsuarioModel UsuarioFormulario { get; set; }
        public string RolFiltroSeleccionado { get; set; }

        // Estados del formulario
        public bool MostrarFormulario { get; set; } = false;
        public bool EsEdicion { get; set; } = false;
        public string TituloFormulario => EsEdicion ? "Editar Usuario" : "Crear Usuario";
        public string TextoBotonGuardar => EsEdicion ? "Actualizar" : "Crear";

        // Comandos
        public ICommand CrearUsuarioCommand { get; set; }
        public ICommand EditarUsuarioCommand { get; set; }
        public ICommand EliminarUsuarioCommand { get; set; }
        public ICommand GuardarUsuarioCommand { get; set; }
        public ICommand CancelarFormularioCommand { get; set; }
        public ICommand VolverCommand { get; set; }

        public GestionUsuariosViewModel(INavigation navigation)
        {
            _navigation = navigation;
            _repository = new UsuariosRepository();

            InitializeCollections();
            InitializeCommands();
            CargarUsuarios();
        }

        private void InitializeCollections()
        {
            TodosLosUsuarios = new ObservableCollection<UsuarioModelExtendido>();
            UsuariosFiltrados = new ObservableCollection<UsuarioModelExtendido>();

            RolesFiltro = new ObservableCollection<string>
            {
                "Todos",
                "Civil",
                "Funcionario",
                "Administrador"
            };

            RolesDisponibles = new ObservableCollection<string>
            {
                "Civil",
                "Funcionario",
                "Administrador"
            };

            RolFiltroSeleccionado = "Todos";
            UsuarioFormulario = new UsuarioModel();
        }

        private void InitializeCommands()
        {
            CrearUsuarioCommand = new Command(MostrarFormularioCreacion);
            EditarUsuarioCommand = new Command<UsuarioModelExtendido>(MostrarFormularioEdicion);
            EliminarUsuarioCommand = new Command<UsuarioModelExtendido>(async (usuario) => await EliminarUsuario(usuario));
            GuardarUsuarioCommand = new Command(async () => await GuardarUsuario());
            CancelarFormularioCommand = new Command(CancelarFormulario);
            VolverCommand = new Command(async () => await _navigation.PopAsync());
        }

        private async Task CargarUsuarios()
        {
            try
            {
                var usuarios = await _repository.GetAllAsync();

                TodosLosUsuarios.Clear();
                foreach (var kvp in usuarios)
                {
                    var usuarioExtendido = new UsuarioModelExtendido
                    {
                        Id = kvp.Value.Id,
                        Key = kvp.Key,
                        Nombre = kvp.Value.Nombre,
                        Correo = kvp.Value.Correo,
                        Contrasena = kvp.Value.Contrasena,
                        Rol = kvp.Value.Rol
                    };
                    TodosLosUsuarios.Add(usuarioExtendido);
                }

                AplicarFiltro();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar usuarios: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudieron cargar los usuarios", "OK");
            }
        }

        private void AplicarFiltro()
        {
            UsuariosFiltrados.Clear();

            var usuariosFiltrados = RolFiltroSeleccionado == "Todos"
                ? TodosLosUsuarios.ToList()
                : TodosLosUsuarios.Where(u => u.Rol == RolFiltroSeleccionado).ToList();

            foreach (var usuario in usuariosFiltrados)
            {
                UsuariosFiltrados.Add(usuario);
            }
        }

        private void MostrarFormularioCreacion()
        {
            EsEdicion = false;
            UsuarioFormulario = new UsuarioModel { Rol = "Funcionario" }; // Por defecto Funcionario
            MostrarFormulario = true;
        }

        private void MostrarFormularioEdicion(UsuarioModelExtendido usuario)
        {
            if (usuario == null) return;

            EsEdicion = true;
            UsuarioFormulario = new UsuarioModel
            {
                Id = usuario.Id,
                Key = usuario.Key,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Contrasena = usuario.Contrasena,
                Rol = usuario.Rol
            };
            MostrarFormulario = true;
        }

        private async Task GuardarUsuario()
        {
            if (!ValidarFormulario())
                return;

            try
            {
                if (EsEdicion)
                {
                    // Actualizar usuario existente
                    await _repository.UpdateDocumentAsync(UsuarioFormulario, UsuarioFormulario.Key);
                    await Application.Current.MainPage.DisplayAlert("Éxito",
                        "Usuario actualizado correctamente", "OK");
                }
                else
                {
                    // Verificar que el correo no exista
                    bool correoExiste = await _repository.EmailExistsAsync(UsuarioFormulario.Correo);
                    if (correoExiste)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error",
                            "Este correo ya está registrado", "OK");
                        return;
                    }

                    // Crear nuevo usuario
                    await _repository.CreateDocumentAsync(UsuarioFormulario);
                    await Application.Current.MainPage.DisplayAlert("Éxito",
                        "Usuario creado correctamente", "OK");
                }

                CancelarFormulario();
                await CargarUsuarios();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar usuario: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudo guardar el usuario", "OK");
            }
        }

        private async Task EliminarUsuario(UsuarioModelExtendido usuario)
        {
            if (usuario == null) return;

            // No permitir eliminar administradores
            if (usuario.Rol == "Administrador")
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pueden eliminar usuarios administradores", "OK");
                return;
            }

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar eliminación",
                $"¿Está seguro que desea eliminar al usuario '{usuario.Nombre}'?",
                "Sí, eliminar",
                "Cancelar");

            if (!confirmar) return;

            try
            {
                await _repository.DeleteDocumentAsync(usuario.Key);
                await Application.Current.MainPage.DisplayAlert("Éxito",
                    "Usuario eliminado correctamente", "OK");

                await CargarUsuarios();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar usuario: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error",
                    "No se pudo eliminar el usuario", "OK");
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(UsuarioFormulario.Nombre))
            {
                Application.Current.MainPage.DisplayAlert("Error", "El nombre es obligatorio", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UsuarioFormulario.Correo))
            {
                Application.Current.MainPage.DisplayAlert("Error", "El correo es obligatorio", "OK");
                return false;
            }

            if (!Regex.IsMatch(UsuarioFormulario.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Formato de correo inválido", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UsuarioFormulario.Contrasena))
            {
                Application.Current.MainPage.DisplayAlert("Error", "La contraseña es obligatoria", "OK");
                return false;
            }

            if (UsuarioFormulario.Contrasena.Length < 6)
            {
                Application.Current.MainPage.DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UsuarioFormulario.Rol))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Debe seleccionar un rol", "OK");
                return false;
            }

            return true;
        }

        private void CancelarFormulario()
        {
            MostrarFormulario = false;
            EsEdicion = false;
            UsuarioFormulario = new UsuarioModel();
        }

        // Reemplaza el método parcial y el llamado a un método inexistente por una implementación directa.
        // Plan:
        // 1. Elimina la declaración 'partial' y haz el método privado.
        // 2. Implementa el filtrado usando el método ya existente 'AplicarFiltro'.
        // 3. Llama a 'AplicarFiltro' cuando cambie el filtro.

        private void OnRolFiltroSeleccionadoChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                AplicarFiltro();
            }
        }

        // Clase extendida para incluir propiedades adicionales de UI
        public class UsuarioModelExtendido : UsuarioModel
        {
            public Color RolColor => Rol switch
            {
                "Administrador" => Color.FromArgb("#E31E24"), // Rojo
                "Funcionario" => Color.FromArgb("#1649A1"),   // Azul
                "Civil" => Color.FromArgb("#28A745"),         // Verde
                _ => Color.FromArgb("#6C757D")                // Gris
            };

            public bool PuedeEliminar => Rol != "Administrador";
        }
    }
}
 
