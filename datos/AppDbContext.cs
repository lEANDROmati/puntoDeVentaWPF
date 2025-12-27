
using Entidades;
using Microsoft.EntityFrameworkCore;


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
            // Cadena de conexión a SQL Server Local.
            // Database=MiPOS_DB será el nombre de tu base de datos.
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MiPOS_DB;Trusted_Connection=True;TrustServerCertificate=True;");
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
                     ImprimirTicket = true,
                     UsarControlCaja = true,

                     // --- ESTOS SON LOS QUE FALTABAN ---
                     Cuit = "00-00000000-0",
                     Direccion = "Sin Dirección Registrada"
                 }
                 );

            // Crear Usuario Admin por defecto
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NombreUsuario = "admin",
                    Password = "123",
                    NombreCompleto = "Administrador",
                    Rol = "Admin",
                    Activo = true
                }
            );
        }
    }
}
