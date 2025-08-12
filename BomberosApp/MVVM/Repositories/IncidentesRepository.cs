using BomberosApp.MVVM.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace BomberosApp.MVVM.Repositories
{
    public class IncidentesRepository
    {
        private readonly FirebaseClient _client;

        public IncidentesRepository()
        {
            _client = new FirebaseClient("https://fir-maui-b923e-default-rtdb.firebaseio.com/");
        }

        public async Task CrearIncidenteAsync(IncidenteModel incidente)
        {
            await _client
                .Child("Incidentes")
                .PostAsync(incidente);
        }

        public async Task<List<FirebaseObject<IncidenteModel>>> ObtenerTodosAsync()
        {
            return (await _client
                .Child("Incidentes")
                .OnceAsync<IncidenteModel>())
                .ToList();
        }

        public async Task EliminarIncidenteAsync(string key)
        {
            await _client
                .Child("Incidentes")
                .Child(key)
                .DeleteAsync();
        }

        // Puedes añadir más métodos según se necesiten (por ID, por fecha, etc.)
    }
}
