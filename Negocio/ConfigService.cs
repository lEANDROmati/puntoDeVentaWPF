using Datos;
using Entidades;
using System.Linq;

namespace Negocio
{
    public class ConfigService
    {
        public Configuracion ObtenerConfig()
        {
            using (var db = new AppDbContext())
            {
                var config = db.Configuraciones.FirstOrDefault();
                if (config == null)
                {
                    // Si no existe, creamos la por defecto
                    config = new Configuracion();
                    db.Configuraciones.Add(config);
                    db.SaveChanges();
                }
                return config;
            }
        }

        // Método para guardar cambios desde la pantalla de ajustes
        public void GuardarConfig(Configuracion c)
        {
            using (var db = new AppDbContext())
            {
                db.Configuraciones.Update(c);
                db.SaveChanges();
            }
        }
    }
}