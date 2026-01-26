using Datos;
using Entidades;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Negocio
{
    public class ConfigService
    {
        public async Task<Configuracion> ObtenerConfigAsync()
        {
            using (var db = new AppDbContext())
            {
                var config = await db.Configuraciones.FirstOrDefaultAsync();
                if (config == null)
                {
                    // Si no existe, creamos la por defecto
                    config = new Configuracion();
                    db.Configuraciones.Add(config);
                    await db.SaveChangesAsync();
                }
                return config;
            }
        }

        // Método para guardar cambios desde la pantalla de configuración
        public async Task GuardarConfigAsync(Configuracion c)
        {
            using (var db = new AppDbContext())
            {
                db.Configuraciones.Update(c);
                await db.SaveChangesAsync();
            }
        }
    }
}