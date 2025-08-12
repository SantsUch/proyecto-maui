using BomberosApp.MVVM.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace BomberosApp.MVVM.Repositories
{
    public class UsuariosRepository
    {
        private readonly FirebaseClient _client;

        public UsuariosRepository()
        {
            _client = new FirebaseClient("https://fir-maui-432d5-default-rtdb.firebaseio.com/");
        }

        // 1. Crear usuario
        public async Task CreateDocumentAsync(UsuarioModel usuario)
        {
            await _client
                .Child("Usuarios")
                .PostAsync(usuario);

            Console.WriteLine($"Usuario {usuario.Nombre} creado correctamente.");
        }

        // 2. Obtener todos los usuarios como diccionario
        public async Task<Dictionary<string, UsuarioModel>> GetAllAsync()
        {
            try
            {
                var listaUsuarios = await _client.Child("Usuarios").OnceAsync<UsuarioModel>();
                var usuariosDict = new Dictionary<string, UsuarioModel>();

                foreach (var usuario in listaUsuarios)
                {
                    usuariosDict.Add(usuario.Key, usuario.Object);
                }

                return usuariosDict;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar los usuarios: {ex.Message}");
                return new Dictionary<string, UsuarioModel>();
            }
        }

        // 3. Obtener usuario por correo
        public async Task<UsuarioModel> GetByEmailAsync(string email)
        {
            var usuarios = await GetAllAsync();

            return usuarios.Values.FirstOrDefault(u => u.Correo?.Trim().ToLower() == email.Trim().ToLower());
        }

        // 4. Actualizar
        public async Task UpdateDocumentAsync(UsuarioModel usuario, string key)
        {
            await _client
                .Child("Usuarios")
                .Child(key)
                .PutAsync(usuario);

            Console.WriteLine($"Usuario {usuario.Nombre} actualizado correctamente.");
        }

        // 5. Eliminar
        public async Task DeleteDocumentAsync(string key)
        {
            await _client
                .Child("Usuarios")
                .Child(key)
                .DeleteAsync();

            Console.WriteLine($"Usuario eliminado correctamente.");
        }

        // 6. Verificar si un correo ya existe
        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                var usuarios = await GetAllAsync();

                return usuarios.Values.Any(u =>
                    u.Correo?.Trim().ToLower() == email.Trim().ToLower());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar si el correo existe: {ex.Message}");
                return false;
            }
        }
    }
}

