using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entidades;
using Negocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace puntoDeVenta.ViewModels
{
    public partial class HistorialViewModel : ObservableObject
    {
        private readonly VentaService _ventaService;

        // Filtros de fecha (Por defecto: Hoy)
        [ObservableProperty] private DateTime fechaInicio = DateTime.Now;
        [ObservableProperty] private DateTime fechaFin = DateTime.Now;

        // Lista de ventas para la tabla
        [ObservableProperty] private ObservableCollection<Venta> listaVentas;

        // Venta seleccionada (para ver detalles futuro)
        [ObservableProperty] private Venta ventaSeleccionada;

        // Totales del período
        [ObservableProperty] private decimal totalVendido;
        [ObservableProperty] private int cantidadVentas;

        public HistorialViewModel()
        {
            _ventaService = new VentaService();
            ListaVentas = new ObservableCollection<Venta>();

            // Buscar automáticamente al entrar
            BuscarVentas();
        }

        [RelayCommand]
        private void BuscarVentas()
        {
            try
            {
                var ventas = _ventaService.GetVentasPorFecha(FechaInicio, FechaFin);

                ListaVentas.Clear();
                foreach (var v in ventas) ListaVentas.Add(v);

                // Calcular resumen
                TotalVendido = ListaVentas.Sum(v => v.Total);
                CantidadVentas = ListaVentas.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar historial: " + ex.Message);
            }
        }

        [RelayCommand]
        private void VerDetalle()
        {
            if (VentaSeleccionada != null)
            {
                // Por ahora un mensaje simple, luego podemos hacer una ventanita de ticket
                string msg = $"Venta #{VentaSeleccionada.Id}\n" +
                             $"Fecha: {VentaSeleccionada.Fecha}\n" +
                             $"Pago: {VentaSeleccionada.MetodoPago}\n\n";

                foreach (var d in VentaSeleccionada.Detalles)
                {
                    string prod = d.Producto != null ? d.Producto.Nombre : "(Eliminado)";
                    msg += $"- {d.Cantidad} x {prod} (${d.Subtotal})\n";
                }

                msg += $"\nTOTAL: ${VentaSeleccionada.Total}";
                MessageBox.Show(msg, "Detalle de Venta");
            }
        }
    }
}
