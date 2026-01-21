using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Negocio;
using Negocio.DTO; // Asegúrate de importar el DTO
using puntoDeVenta.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace puntoDeVenta.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly VentaService _ventaService;
        private readonly ProductoService _productoService;
        private readonly CajaService _cajaService;

        // --- MÉTRICAS GENERALES ---
        [ObservableProperty] private decimal ventasHoy;
        [ObservableProperty] private int cantidadTicketsHoy;
        [ObservableProperty] private int productosBajoStock;
        [ObservableProperty] private string mensajeRendimiento;

        // --- GRÁFICO DINÁMICO ---
        [ObservableProperty]
        private ObservableCollection<DatoGrafico> datosGrafico;

        [ObservableProperty]
        private string tituloGrafico; // Ej: "Rendimiento: Últimos 7 días"

        public DashboardViewModel()
        {
            _ventaService = new VentaService();
            _productoService = new ProductoService();
            _cajaService = new CajaService();
            DatosGrafico = new ObservableCollection<DatoGrafico>();

            CargarMetricas();

            // Por defecto cargamos la semana
            FiltrarSemana();
        }

        public void CargarMetricas()
        {
            try
            {
                var ventas = _ventaService.GetVentasPorFecha(DateTime.Today, DateTime.Now);
                VentasHoy = ventas.Sum(v => v.Total);
                CantidadTicketsHoy = ventas.Count;

                var productos = _productoService.GetAll();
                ProductosBajoStock = productos.Count(p => p.EstadoStock == "STOCK BAJO");

                MensajeRendimiento = VentasHoy > 0 ? "¡Ventas activas!" : "Esperando primera venta...";
            }
            catch (Exception)
            {
                VentasHoy = 0;
            }
        }

        // --- COMANDOS DE FILTROS ---

        [RelayCommand]
        private void FiltrarDia()
        {
            TituloGrafico = "Rendimiento: Hoy (Por Hora)";
            CargarGrafico(DateTime.Today, DateTime.Now, esPorHora: true);
        }

        [RelayCommand]
        private void FiltrarSemana()
        {
            TituloGrafico = "Rendimiento: Últimos 7 Días";
            // Desde hace 6 días hasta hoy (total 7)
            CargarGrafico(DateTime.Today.AddDays(-6), DateTime.Now, esPorHora: false);
        }

        [RelayCommand]
        private void FiltrarQuincena()
        {
            TituloGrafico = "Rendimiento: Últimos 15 Días";
            CargarGrafico(DateTime.Today.AddDays(-14), DateTime.Now, esPorHora: false);
        }

        // --- LÓGICA DEL GRÁFICO ---
        private void CargarGrafico(DateTime desde, DateTime hasta, bool esPorHora)
        {
            try
            {
                // 1. Obtener ventas crudas del rango
                var ventas = _ventaService.GetVentasPorFecha(desde, hasta);
                DatosGrafico.Clear();

                // 2. Definir el eje X (las etiquetas)
                // Si es por HORA (08, 09, 10...) o por DÍA (Lun, Mar...)
                var datosAgrupados = new System.Collections.Generic.List<DatoGrafico>();

                if (esPorHora)
                {
                    // Crear slots para las horas operativas (ej: 08:00 a 22:00)
                    for (int i = 8; i <= 22; i++)
                    {
                        decimal totalHora = ventas.Where(v => v.Fecha.Hour == i).Sum(v => v.Total);
                        datosAgrupados.Add(new DatoGrafico
                        {
                            Etiqueta = $"{i}:00",
                            ValorReal = totalHora,
                            EsHoy = (DateTime.Now.Hour == i) // Resaltar hora actual
                        });
                    }
                }
                else
                {
                    // Crear slots por cada día del rango
                    for (var dia = desde.Date; dia <= hasta.Date; dia = dia.AddDays(1))
                    {
                        decimal totalDia = ventas.Where(v => v.Fecha.Date == dia).Sum(v => v.Total);

                        // Formato etiqueta: "Lun 12" o solo "12" si son muchos
                        string formato = (hasta - desde).TotalDays > 10 ? "dd/MM" : "ddd dd";

                        datosAgrupados.Add(new DatoGrafico
                        {
                            Etiqueta = dia.ToString(formato),
                            ValorReal = totalDia,
                            EsHoy = (dia == DateTime.Today) // Resaltar día actual
                        });
                    }
                }

                // 3. Normalizar Alturas (Escalar al tamaño del contenedor, ej: 150px)
                decimal maxValor = datosAgrupados.Any() ? datosAgrupados.Max(d => d.ValorReal) : 1;
                if (maxValor == 0) maxValor = 1; // Evitar división por cero

                double alturaMaximaContenedor = 150; // Pixeles disponibles en la vista

                foreach (var d in datosAgrupados)
                {
                    // Regla de tres simple: Si MaxValor es 150px, ValorActual es X
                    d.Altura = (double)(d.ValorReal / maxValor) * alturaMaximaContenedor;

                    // Asegurar un mínimo de 2px para que se vea la barra aunque sea 0
                    if (d.Altura < 2) d.Altura = 2;

                    // Color: Azul fuerte para hoy/hora actual, Azul suave para histórico
                    d.Color = d.EsHoy ? "#3F51B5" : "#9FA8DA";
                    if (d.ValorReal == 0) d.Color = "#E0E0E0"; // Gris si es cero

                    DatosGrafico.Add(d);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculando gráfico: " + ex.Message);
            }
        }

        // ... MANTENER EL RESTO DE TUS MÉTODOS (CerrarCaja, etc) ...
        [RelayCommand]
        private void CerrarCaja()
        {
            try
            {
                // 1. Obtener la sesión de caja actual
                var cajaActual = _cajaService.ObtenerCajaAbierta();

                if (cajaActual == null)
                {
                    MessageBox.Show("No hay ninguna caja abierta para cerrar.", "Aviso");
                    return;
                }

                // ========================================================================
                // 🧠 CÁLCULO AUTOMÁTICO DEL MONTO ESPERADO
                // ========================================================================

                // A. Traemos todas las ventas del día (usamos DateTime.Now para cerrar hasta el momento actual)
                var ventasDelDia = _ventaService.GetVentasPorFecha(cajaActual.FechaApertura, DateTime.Now);

                // B. FILTRO DE SEGURIDAD: 
                // Aseguramos que las ventas sean POSTERIORES a la hora de apertura exacta
                // (Esto evita sumar ventas de un turno anterior si hubo dos cajeros el mismo día)
                var ventasDeEstaSesion = ventasDelDia.Where(v => v.Fecha >= cajaActual.FechaApertura).ToList();

                // C. Sumamos SOLO lo que entró en EFECTIVO
                // (Las tarjetas o QR van al banco, no están en el cajón de dinero)
                decimal totalVentasEfectivo = ventasDeEstaSesion
                                                .Where(v => v.MetodoPago == "Efectivo")
                                                .Sum(v => v.Total);

                // D. Fórmula Final: Lo que había + Lo que entró
                decimal montoEsperadoSistema = cajaActual.MontoInicial + totalVentasEfectivo;

                // ========================================================================

                // 2. Abrimos la ventana pasándole el dato calculado
                // IMPORTANTE: Tu ventana 'CierreCajaWindow' debe tener el constructor que acepta el decimal
                var ventanaCierre = new Views.CierreCajaWindow(montoEsperadoSistema);

                if (ventanaCierre.ShowDialog() == true)
                {
                    // 3. El usuario contó el dinero real y confirmó
                    decimal montoRealUsuario = ventanaCierre.MontoRealEnCaja;

                    // 4. Guardamos el cierre
                    _cajaService.CerrarCaja(montoRealUsuario);

                    // 5. Calculamos la diferencia final para el reporte
                    decimal diferencia = montoRealUsuario - montoEsperadoSistema;
                    string estado = diferencia == 0 ? "PERFECTO" : (diferencia > 0 ? "SOBRANTE" : "FALTANTE");

                    MessageBox.Show($"Caja cerrada con éxito.\n\n" +
                                    $"Esperado: ${montoEsperadoSistema:N2}\n" +
                                    $"Real: ${montoRealUsuario:N2}\n" +
                                    $"Estado: {estado} (${diferencia:N2})",
                                    "Reporte Z", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 6. Recargar Dashboard
                    CargarMetricas();
                    FiltrarDia();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar caja: {ex.Message}");
            }
        }
    }
}