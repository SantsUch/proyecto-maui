using PropertyChanged;

namespace BomberosApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class IncidenteModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Key { get; set; } = string.Empty; // Para Firebase key
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Imagen { get; set; } = string.Empty;
        public DateTime FechaReportado { get; set; } = DateTime.Now;

        // Campos para asignación
        public string Estado { get; set; } = "Reportado";
        public string FuncionarioAsignadoId { get; set; } = string.Empty;
        public string FuncionarioAsignadoNombre { get; set; } = string.Empty;
        public DateTime? FechaAsignacion { get; set; }
        public string ObservacionesAsignacion { get; set; } = string.Empty;

        // Campo para asociar con el usuario que reportó
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;

        // Estados posibles del incidente
        public static class Estados
        {
            public const string Reportado = "Reportado";
            public const string Asignado = "Asignado";
            public const string EnProceso = "En Proceso";
            public const string Resuelto = "Resuelto";
            public const string Cancelado = "Cancelado";
        }

        // Prioridades posibles
        public static class Prioridades
        {
            public const string Critica = "Crítica";
            public const string Alta = "Alta";
            public const string Media = "Media";
            public const string Baja = "Baja";
        }
    }
}