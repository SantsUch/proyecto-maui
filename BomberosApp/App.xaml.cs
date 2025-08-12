using BomberosApp.MVVM.Views;

namespace BomberosApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new InicioView());
        }
    }
}