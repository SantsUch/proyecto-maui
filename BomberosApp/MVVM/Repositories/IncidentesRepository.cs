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
            _client = new FirebaseClient("https://fir-maui-432d5-default-rtdb.firebaseio.com/");
        }

        public async Task CrearIncidenteAsync(IncidenteModel incidente)
        {
            await _client
                .Child("Incidentes")
                .PostAsync(incidente);
        }

        public async Task<List<FirebaseObject<IncidenteModel>>> ObtenerTodosAsync()
        {
            try
            {
                return (await _client
                    .Child("Incidentes")
                    .OnceAsync<IncidenteModel>())
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener incidentes: {ex.Message}");
                return new List<FirebaseObject<IncidenteModel>>();
            }
        }

        public async Task<IncidenteModel> ObtenerPorIdAsync(string id)
        {
            var incidentes = await ObtenerTodosAsync();
            return incidentes.FirstOrDefault(i => i.Object.Id == id)?.Object;
        }

        public async Task<List<IncidenteModel>> ObtenerPorUsuarioAsync(string usuarioId)
        {
            var incidentes = await ObtenerTodosAsync();
            return incidentes
                .Where(i => i.Object.UsuarioId == usuarioId)
                .Select(i => i.Object)
                .OrderByDescending(i => i.FechaReportado)
                .ToList();
        }

        public async Task<List<IncidenteModel>> ObtenerPorEstadoAsync(string estado)
        {
            var incidentes = await ObtenerTodosAsync();
            return incidentes
                .Where(i => i.Object.Estado == estado)
                .Select(i => i.Object)
                .OrderByDescending(i => i.FechaReportado)
                .ToList();
        }

        public async Task ActualizarIncidenteAsync(IncidenteModel incidente)
        {
            try
            {
                // Buscar el incidente por su ID para obtener la key de Firebase
                var incidentes = await ObtenerTodosAsync();
                var incidenteExistente = incidentes.FirstOrDefault(i => i.Object.Id == incidente.Id);

                if (incidenteExistente != null)
                {
                    await _client
                        .Child("Incidentes")
                        .Child(incidenteExistente.Key)
                        .PutAsync(incidente);
                }
                else
                {
                    throw new Exception("Incidente no encontrado para actualizar");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar incidente: {ex.Message}");
                throw;
            }
        }


        public async Task EliminarIncidenteAsync(string incidenteId)
        {
            try
            {
                var incidentes = await ObtenerTodosAsync();
                var incidenteAEliminar = incidentes.FirstOrDefault(i => i.Object.Id == incidenteId);

                if (incidenteAEliminar != null)
                {
                    if (incidenteAEliminar.Object.Estado == IncidenteModel.Estados.Resuelto)
                    {
                        await _client
                            .Child("Incidentes")
                            .Child(incidenteAEliminar.Key)
                            .DeleteAsync();
                    }
                    else
                    {
                        throw new Exception("No se puede eliminar un incidente que no esté resuelto.");
                    }
                }
                else
                {
                    throw new Exception("Incidente no encontrado para eliminar");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar incidente: {ex.Message}");
                throw;
            }
        }

        public async Task AsignarIncidenteAsync(string incidenteId, string funcionarioId, string funcionarioNombre, string categoria, string prioridad, string observaciones = "")
        {
            var incidentes = await ObtenerTodosAsync();
            var incidenteFirebase = incidentes.FirstOrDefault(i => i.Object.Id == incidenteId);

            if (incidenteFirebase != null)
            {
                var incidente = incidenteFirebase.Object;
                incidente.FuncionarioAsignadoId = funcionarioId;
                incidente.FuncionarioAsignadoNombre = funcionarioNombre;
                incidente.Categoria = categoria;
                incidente.Prioridad = prioridad;
                incidente.Estado = "Asignado";
                incidente.FechaAsignacion = DateTime.Now;
                incidente.ObservacionesAsignacion = observaciones;

                await _client
                    .Child("Incidentes")
                    .Child(incidenteFirebase.Key)
                    .PutAsync(incidente);
            }
            else
            {
                throw new Exception("Incidente no encontrado para asignar");
            }
        }

        public async Task CambiarEstadoIncidenteAsync(string incidenteId, string nuevoEstado)
        {
            var incidentes = await ObtenerTodosAsync();
            var incidenteFirebase = incidentes.FirstOrDefault(i => i.Object.Id == incidenteId);

            if (incidenteFirebase != null)
            {
                var incidente = incidenteFirebase.Object;
                incidente.Estado = nuevoEstado;

                await _client
                    .Child("Incidentes")
                    .Child(incidenteFirebase.Key)
                    .PutAsync(incidente);
            }
            else
            {
                throw new Exception("Incidente no encontrado para cambiar estado");
            }
        }
    }
}