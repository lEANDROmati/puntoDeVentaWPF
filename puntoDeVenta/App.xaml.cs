using Datos;
using puntoDeVenta.Views;
using Microsoft.EntityFrameworkCore;
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

            using (var context = new AppDbContext())
            {
                // Crea la base de datos y aplica todas las migraciones pendientes
                // (Incluyendo la creación del usuario Admin)
                context.Database.Migrate();

                DbSeeder.Inicializar(context);
            }

            // Abrir primero el Login
            var login = new LoginWindow();
            login.Show();
        }
    }

}
