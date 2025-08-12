using PropertyChanged;

namespace BomberosApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class IncidenteModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Imagen { get; set; } = string.Empty;
        public DateTime FechaReportado { get; set; } = DateTime.Now;
    }
}
