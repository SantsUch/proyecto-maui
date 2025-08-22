using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views
{
    public partial class VisualizarIncidentesView : ContentPage
    {
        public VisualizarIncidentesView(UsuarioModel usuario)
        {
            InitializeComponent();
            BindingContext = new VisualizarIncidentesViewModel(Navigation, usuario);
        }
    }
}