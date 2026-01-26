using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Negocio;
using Negocio.DTO; 
using puntoDeVenta.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
        private string tituloGrafico; 

        public DashboardViewModel()
        {
            _ventaService = new VentaService();
            _productoService = new ProductoService();
            _cajaService = new CajaService();
            DatosGrafico = new ObservableCollection<DatoGrafico>();

            
            _ = InicializarDashboard();
        }

        private async Task InicializarDashboard()
        {
            await CargarMetricas();
            await FiltrarSemana();
        }

        public async Task CargarMetricas()
        {
            try
            {
                var ventas = await _ventaService.GetVentasPorFechaAsync(DateTime.Today, DateTime.Now);
                VentasHoy = ventas.Sum(v => v.Total);
                CantidadTicketsHoy = ventas.Count;

                var productos = await _productoService.GetAllAsync();
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
        private async Task FiltrarDia()
        {
            TituloGrafico = "Rendimiento: Hoy (Por Hora)";
            await CargarGrafico(DateTime.Today, DateTime.Now, esPorHora: true);
        }

        [RelayCommand]
        private async Task FiltrarSemana()
        {
            TituloGrafico = "Rendimiento: Últimos 7 Días";
            // Desde hace 6 días hasta hoy (total 7)
            await CargarGrafico(DateTime.Today.AddDays(-6), DateTime.Now, esPorHora: false);
        }

        [RelayCommand]
        private async Task FiltrarQuincena()
        {
            TituloGrafico = "Rendimiento: Últimos 15 Días";
            await CargarGrafico(DateTime.Today.AddDays(-14), DateTime.Now, esPorHora: false);
        }

        // --- LÓGICA DEL GRÁFICO ---
        private async Task CargarGrafico(DateTime desde, DateTime hasta, bool esPorHora)
        {
            try
            {
                // 1. Obtener ventas crudas del rango
                var ventas = await _ventaService.GetVentasPorFechaAsync(desde, hasta);
                DatosGrafico.Clear();

                
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
                            EsHoy = (DateTime.Now.Hour == i) 
                        });
                    }
                }
                else
                {
                    // Crear slots por cada día del rango
                    for (var dia = desde.Date; dia <= hasta.Date; dia = dia.AddDays(1))
                    {
                        decimal totalDia = ventas.Where(v => v.Fecha.Date == dia).Sum(v => v.Total);

                        // Formato etiqueta:
                        string formato = (hasta - desde).TotalDays > 10 ? "dd/MM" : "ddd dd";

                        datosAgrupados.Add(new DatoGrafico
                        {
                            Etiqueta = dia.ToString(formato),
                            ValorReal = totalDia,
                            EsHoy = (dia == DateTime.Today) 
                        });
                    }
                }

                
                decimal maxValor = datosAgrupados.Any() ? datosAgrupados.Max(d => d.ValorReal) : 1;
                if (maxValor == 0) maxValor = 1; 

                double alturaMaximaContenedor = 150; 

                foreach (var d in datosAgrupados)
                {
                    // Regla de tres simple
                    d.Altura = (double)(d.ValorReal / maxValor) * alturaMaximaContenedor;

                   
                    if (d.Altura < 2) d.Altura = 2;

                   
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

       
        [RelayCommand]
        private async Task CerrarCaja()
        {
            try
            {
                // 1. Obtener la sesión de caja actual
                var cajaActual = await _cajaService.ObtenerCajaAbiertaAsync();

                if (cajaActual == null)
                {
                    MessageBox.Show("No hay ninguna caja abierta para cerrar.", "Aviso");
                    return;
                }

               
                //. Traemos todas las ventas del día (usamos DateTime.Now para cerrar hasta el momento actual)
                var ventasDelDia = await _ventaService.GetVentasPorFechaAsync(cajaActual.FechaApertura, DateTime.Now);

                
                var ventasDeEstaSesion = ventasDelDia.Where(v => v.Fecha >= cajaActual.FechaApertura).ToList();

                
                decimal totalVentasEfectivo = ventasDeEstaSesion
                                                .Where(v => v.MetodoPago == "Efectivo")
                                                .Sum(v => v.Total);

                
                decimal montoEsperadoSistema = cajaActual.MontoInicial + totalVentasEfectivo;

                // ========================================================================

               
                var ventanaCierre = new Views.CierreCajaWindow(montoEsperadoSistema);

                if (ventanaCierre.ShowDialog() == true)
                {
                    
                    decimal montoRealUsuario = ventanaCierre.MontoRealEnCaja;

                   
                    await _cajaService.CerrarCajaAsync(montoRealUsuario);

                  
                    decimal diferencia = montoRealUsuario - montoEsperadoSistema;
                    string estado = diferencia == 0 ? "PERFECTO" : (diferencia > 0 ? "SOBRANTE" : "FALTANTE");

                    MessageBox.Show($"Caja cerrada con éxito.\n\n" +
                                    $"Esperado: ${montoEsperadoSistema:N2}\n" +
                                    $"Real: ${montoRealUsuario:N2}\n" +
                                    $"Estado: {estado} (${diferencia:N2})",
                                    "Reporte Z", MessageBoxButton.OK, MessageBoxImage.Information);

                    
                    await CargarMetricas();
                    await FiltrarDia();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar caja: {ex.Message}");
            }
        }
    }
}