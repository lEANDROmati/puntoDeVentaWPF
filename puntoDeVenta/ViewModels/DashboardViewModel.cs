using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Negocio;
using System.Windows;

namespace puntoDeVenta.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly VentaService _ventaService;
        private readonly ProductoService _productoService;
        private readonly CajaService _cajaService;

        // --- PROPIEDADES QUE SE VEN EN PANTALLA ---
        [ObservableProperty] private decimal ventasHoy;
        [ObservableProperty] private int cantidadTicketsHoy;
        [ObservableProperty] private int productosBajoStock;


        // Propiedades extra para comparar (opcional)
        [ObservableProperty] private string mensajeRendimiento; // Ej: "+10% vs ayer"

        public DashboardViewModel()
        {
            _ventaService = new VentaService();
            _productoService = new ProductoService();
            _cajaService = new CajaService();

            // Cargamos datos al iniciar (aunque el evento Loaded lo hará de nuevo)
            CargarMetricas();
        }

        public void CargarMetricas()
        {
            try
            {
                // 1. Obtener ventas de HOY (Desde las 00:00 hasta ahora)
                var ventas = _ventaService.GetVentasPorFecha(DateTime.Today, DateTime.Now);

                VentasHoy = ventas.Sum(v => v.Total);
                CantidadTicketsHoy = ventas.Count;

                // 2. Obtener productos con Stock Bajo
                // Usamos GetAll porque ya tiene la lógica de "STOCK BAJO" calculada en el DTO
                var productos = _productoService.GetAll();
                ProductosBajoStock = productos.Count(p => p.EstadoStock == "STOCK BAJO");

                // 3. Mensajito motivador (Lógica simple)
                if (VentasHoy > 0)
                    MensajeRendimiento = "¡Ventas activas!";
                else
                    MensajeRendimiento = "Esperando primera venta...";

            }
            catch (Exception)
            {
                // Si falla, mostramos ceros para no romper la app
                VentasHoy = 0;
                ProductosBajoStock = 0;
            }
        }

        [RelayCommand]
        private void CerrarCaja()
        {
            try
            {
                var caja = _cajaService.ObtenerCajaAbierta();
                if (caja == null)
                {
                    MessageBox.Show("No hay ninguna caja abierta para cerrar.");
                    return;
                }

                // 1. Calcular totales reales del turno
                // Buscamos ventas hechas DESDE que se abrió la caja HASTA ahora
                var ventasTurno = _ventaService.GetVentasPorFecha(caja.FechaApertura, DateTime.Now);
                decimal totalVendido = ventasTurno.Sum(v => v.Total);
                decimal totalEnCaja = caja.MontoInicial + totalVendido;

                // 2. Preguntar confirmación (Simulación de arqueo)
                var mensaje = $"--- CIERRE DE CAJA ---\n\n" +
                              $"Inicio: {caja.MontoInicial:C2}\n" +
                              $"Ventas: {totalVendido:C2}\n" +
                              $"----------------------\n" +
                              $"TOTAL ESPERADO: {totalEnCaja:C2}\n\n" +
                              $"¿Confirmar cierre de turno?";

                if (MessageBox.Show(mensaje, "Cierre Z", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _cajaService.CerrarCaja(totalEnCaja);
                    MessageBox.Show("¡Turno cerrado correctamente!\nSe ha desconectado la caja.", "Cierre Exitoso");

                    // Opcional: Recargar métricas (volverán a 0 si filtramos por caja abierta, o seguirán igual si es por día)
                    CargarMetricas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cerrar: " + ex.Message);
            }
        }
    }
}