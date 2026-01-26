using Datos;
using puntoDeVenta.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace puntoDeVenta
{
    
    public partial class App : Application
    {
  protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using (var context = new AppDbContext())
            {
                
                context.Database.Migrate();

                DbSeeder.Inicializar(context);
            }

            // Abrir primero el Login
            var login = new LoginWindow();
            login.Show();
        }
    }

}
