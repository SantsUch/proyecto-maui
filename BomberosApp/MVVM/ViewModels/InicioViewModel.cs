using System.Windows.Input;
using BomberosApp.MVVM.Views;

namespace BomberosApp.MVVM.ViewModels
{
    public class InicioViewModel
    {
        public ICommand IrALoginCommand { get; set; }
        public ICommand IrAReporteCommand { get; set; }

        private readonly INavigation _navigation;
        public InicioViewModel(INavigation navigation)
        {
            _navigation = navigation;
            IrALoginCommand = new Command(async () => await _navigation.PushAsync(new LoginView()));
            IrAReporteCommand = new Command(async () => await _navigation.PushAsync(new ReportarIncidenteView()));
        }
        
        /**
        private async void IrALogin()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new LoginView());
        }

        private async void IrAReportar()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ReportarIncidenteView());
        }**/
    }
}
