using puntoDeVenta.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace puntoDeVenta
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
  protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

                // Abrir primero el Login
                    var login = new LoginWindow();
                  login.Show();
        }
    }

}
