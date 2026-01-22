
using Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO; 

namespace Datos
{
    public class AppDbContext : DbContext
    {
        // Esta línea le dice a EF que cree una tabla 
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
            // LÓGICA PARA MOVER LA DB A UNA CARPETA SEGURA (APPDATA)
            // -----------------------------------------------------------------------

            // 1. Definimos el nombre de la base de datos
            string nombreBD = "MiPuntoVenta.db";

            // 2. Buscamos la ruta segura del usuario: C:\Users\TuUsuario\AppData\Local\MiPuntoVenta
            string carpetaUsuario = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string carpetaMiApp = Path.Combine(carpetaUsuario, "MiPuntoVentaData"); // Carpeta propia para tu app
            string rutaFinalDB = Path.Combine(carpetaMiApp, nombreBD);

            // 3. Si la carpeta en AppData no existe, la creamos
            if (!Directory.Exists(carpetaMiApp))
            {
                Directory.CreateDirectory(carpetaMiApp);
            }

            // 4. Si la base de datos NO existe en AppData (es la primera vez que corre),
            //    la buscamos en la carpeta de instalación y la copiamos.
            if (!File.Exists(rutaFinalDB))
            {
                // Ruta de instalación (donde está el .exe)
                string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
                string rutaOriginalDB = Path.Combine(directorioBase, nombreBD);

                // Si existe el archivo "molde" que dejó el instalador, lo copiamos
                if (File.Exists(rutaOriginalDB))
                {
                    File.Copy(rutaOriginalDB, rutaFinalDB);
                }
            }

            // 5. Nos conectamos a la base de datos que está en AppData
            optionsBuilder.UseSqlite($"Data Source={rutaFinalDB}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Crear Categoría por defecto
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