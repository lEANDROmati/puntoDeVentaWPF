
using Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO; 

namespace Datos
{
    public class AppDbContext : DbContext
    {
       
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Configuracion> Configuraciones { get; set; }
        public DbSet<CajaSesion> CajasSesiones { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // -----------------------------------------------------------------------
            //  MOVER LA DB A UNA CARPETA (APPDATA)
            // -----------------------------------------------------------------------

            
            string nombreBD = "MiPuntoVenta.db";

           
            string carpetaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string carpetaMiApp = Path.Combine(carpetaUsuario, "MiPuntoVentaData"); 
            string rutaFinalDB = Path.Combine(carpetaMiApp, nombreBD);

            // AppData no existe, se crea la carpeta
            if (!Directory.Exists(carpetaMiApp))
            {
                Directory.CreateDirectory(carpetaMiApp);
            }

            // Si la base de datos NO existe en AppData (es la primera vez que corre)
            
            if (!File.Exists(rutaFinalDB))
            {
                // Ruta de instalación (donde está el .exe)
                string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
                string rutaOriginalDB = Path.Combine(directorioBase, nombreBD);

                // Si existe el archivo que dejó el instalador, lo copiamos
                if (File.Exists(rutaOriginalDB))
                {
                    File.Copy(rutaOriginalDB, rutaFinalDB);
                }
            }

            // conectamos a la base de datos que está en AppData
            optionsBuilder.UseSqlite($"Data Source={rutaFinalDB}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "General", Activo = true }
            );

            // Crear Unidades por defecto
            modelBuilder.Entity<UnidadMedida>().HasData(
                new UnidadMedida { Id = 1, Nombre = "Unidad", Abreviatura = "un", Activo = true },
                new UnidadMedida { Id = 2, Nombre = "Kilogramo", Abreviatura = "kg", Activo = true }
            );

            modelBuilder.Entity<Configuracion>().HasData(
                new Configuracion
                {
                    Id = 1,
                    NombreNegocio = "Mi Negocio",
                    PorcentajeIVA = 21,
                    ManejarIVA = false,
                    ImprimirTicket = false,
                    UsarControlCaja = true,
                    Cuit = "00-00000000-0",
                    Direccion = "Sin Dirección Registrada"
                }
                 );

            
           
        }
    }
}