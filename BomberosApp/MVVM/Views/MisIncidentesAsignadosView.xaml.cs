using BomberosApp.MVVM.ViewModels;
using BomberosApp.MVVM.Models;

namespace BomberosApp.MVVM.Views
{
    public partial class MisIncidentesAsignadosView : ContentPage
    {
        public MisIncidentesAsignadosView(UsuarioModel funcionario)
        {
            InitializeComponent();
            BindingContext = new MisIncidentesAsignadosViewModel(Navigation, funcionario);
        }
    }
}