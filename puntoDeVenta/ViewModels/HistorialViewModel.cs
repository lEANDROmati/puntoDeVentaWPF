using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entidades;
using Negocio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Linq;


namespace puntoDeVenta.ViewModels
{
    public partial class HistorialViewModel : ObservableObject
    {
        private readonly VentaService _ventaService;

       
        [ObservableProperty] private DateTime fechaInicio = DateTime.Now;
        [ObservableProperty] private DateTime fechaFin = DateTime.Now;

        
        [ObservableProperty] private ObservableCollection<Venta> listaVentas;

       
        [ObservableProperty] private Venta ventaSeleccionada;

      
        [ObservableProperty] private decimal totalVendido;
        [ObservableProperty] private int cantidadVentas;

        public HistorialViewModel()
        {
            _ventaService = new VentaService();
            ListaVentas = new ObservableCollection<Venta>();

            
            _ = BuscarVentas();
        }

        [RelayCommand]
        private async Task BuscarVentas()
        {
            try
            {
                var ventas = await _ventaService.GetVentasPorFechaAsync(FechaInicio, FechaFin);

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
