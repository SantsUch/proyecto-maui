using PropertyChanged;

namespace BomberosApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class UsuarioModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Key { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
