using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Negocio;
using Negocio.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace puntoDeVenta.ViewModels
{
    public partial class ReportesViewModel : ObservableObject
    {
        private readonly ReporteService _reporteService;

        // Filtros
        [ObservableProperty] private DateTime fechaInicio = DateTime.Now.AddDays(-30); // Último mes por defecto
        [ObservableProperty] private DateTime fechaFin = DateTime.Now;

        // Listas para las tablas
        [ObservableProperty] private ObservableCollection<ReporteDto> ventasPorCategoria;
        [ObservableProperty] private ObservableCollection<ReporteDto> topProductos;

        public ReportesViewModel()
        {
            _reporteService = new ReporteService();
            VentasPorCategoria = new ObservableCollection<ReporteDto>();
            TopProductos = new ObservableCollection<ReporteDto>();

            // Generar reporte al entrar
            GenerarReporte();
        }

        [RelayCommand]
        private void GenerarReporte()
        {
            try
            {
                // 1. Categorías
                var catData = _reporteService.ObtenerVentasPorCategoria(FechaInicio, FechaFin);
                VentasPorCategoria.Clear();
                foreach (var item in catData) VentasPorCategoria.Add(item);

                // 2. Top Productos
                var topData = _reporteService.ObtenerTopProductos(FechaInicio, FechaFin);
                TopProductos.Clear();
                foreach (var item in topData) TopProductos.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}");
            }
        }
    }
}
