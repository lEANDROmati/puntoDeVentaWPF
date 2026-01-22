using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entidades;
using Microsoft.Win32;
using Negocio;
using Negocio.DTO; 
using puntoDeVenta.Views; 
using System;
using System.Collections.ObjectModel;
using System.IO; 
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace puntoDeVenta.ViewModels
{
    public partial class InventarioViewModel : ObservableObject
    {
        private readonly ProductoService _productoService;

        // 1. LISTA VISIBLE EN PANTALLA (La que cambia al buscar)
        // Le cambiamos el nombre a 'Productos' para que coincida con el XAML
        [ObservableProperty]
        private ObservableCollection<ProductoDto> productos;

        // 2. LISTA "MAESTRA" EN MEMORIA (Respaldo para cuando borras la búsqueda)
        private List<ProductoDto> _listaCompletaRespaldo;

        // 3. PROPIEDAD DEL BUSCADOR
        private string textoBusqueda;
        public string TextoBusqueda
        {
            get => textoBusqueda;
            set
            {
                if (SetProperty(ref textoBusqueda, value))
                {
                    FiltrarResultados(); // ¡Magia! Filtra cada vez que escribes
                }
            }
        }

        public InventarioViewModel()
        {
            _productoService = new ProductoService();
           
            _ = CargarDatos();
        }

        public async Task CargarDatos()
        {
            try
            {
                // 1. El servicio devuelve una lista de DTOs (Datos ya procesados)
                // p.Categoria ya es un string (ej: "Bebidas")
                // p.Unidad ya es un string (ej: "lt")
                var productosDto = await _productoService.GetAllAsync();

                var listaTemporal = new List<ProductoDto>();

                foreach (var p in productosDto)
                {
                    // Recalculamos estado y margen solo para asegurarnos que la visual esté fresca,
                    // aunque el servicio ya suele traerlo.
                    string estado = CalcularEstado(p.Stock, p.StockMinimo);

                    // Protección contra división por cero
                    decimal margen = (p.PrecioCompra == 0) ? 0 : (p.PrecioVenta - p.PrecioCompra) / p.PrecioCompra;

                    listaTemporal.Add(new ProductoDto
                    {
                        Id = p.Id,
                        CodigoBarras = p.CodigoBarras,
                        Nombre = p.Nombre,

                        // --- CORRECCIÓN AQUÍ ---
                        // Antes: p.Categoria.Nombre (Error porque p.Categoria ya es string)
                        // Ahora: p.Categoria (Correcto)
                        Categoria = p.Categoria,

                        // Antes: p.UnidadMedida.Abreviatura (Error porque el DTO no tiene objeto UnidadMedida)
                        // Ahora: p.Unidad (Correcto)
                        Unidad = p.Unidad,
                        // -----------------------

                        PrecioCompra = p.PrecioCompra,
                        PrecioVenta = p.PrecioVenta,
                        Stock = p.Stock,
                        StockMinimo = p.StockMinimo,
                        EstadoStock = estado,
                        MargenGanancia = margen
                    });
                }

                // Guardamos en el respaldo y en la visual
                _listaCompletaRespaldo = listaTemporal;
                Productos = new ObservableCollection<ProductoDto>(_listaCompletaRespaldo);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al cargar inventario: {ex.Message}");
            }
        }

        // --- LÓGICA DEL BUSCADOR ---
        private void FiltrarResultados()
        {
            if (_listaCompletaRespaldo == null) return;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                // Si borró todo, mostramos la lista completa original
                Productos = new ObservableCollection<ProductoDto>(_listaCompletaRespaldo);
            }
            else
            {
                // Filtramos por Nombre O Código
                var filtrados = _listaCompletaRespaldo
                    .Where(p => p.Nombre.ToLower().Contains(TextoBusqueda.ToLower()) ||
                                p.CodigoBarras.Contains(TextoBusqueda))
                    .ToList();

                Productos = new ObservableCollection<ProductoDto>(filtrados);
            }
        }

        // Tu lógica de colores (Perfecta, la dejamos igual)
        private string CalcularEstado(int stock, int minimo)
        {
            if (stock <= minimo) return "STOCK BAJO";
            if (stock <= minimo * 3) return "STOCK MEDIO";
            return "STOCK ALTO";
        }

        // --- COMANDOS ACTUALIZADOS ---

        [RelayCommand]
        private async Task NuevoProducto()
        {
            var ventana = new Views.ProductoFormWindow(); // Asegúrate del namespace correcto
            if (ventana.ShowDialog() == true)
            {
                await CargarDatos();
            }
        }

        // CAMBIO CLAVE: Recibimos el producto como parámetro desde el botón de la fila
        [RelayCommand]
        private async Task EditarProducto(ProductoDto producto)
        {
            if (producto == null) return;

            // Buscamos el producto REAL en la BD
            var productoReal = await _productoService.GetByIdAsync(producto.Id);

            if (productoReal != null)
            {
                var ventana = new Views.ProductoFormWindow(productoReal);
                if (ventana.ShowDialog() == true)
                {
                    await CargarDatos(); // Recargamos para ver los cambios
                    TextoBusqueda = ""; // Limpiamos buscador opcionalmente
                }
            }
        }

        // CAMBIO CLAVE: Recibimos el parámetro
        [RelayCommand]
        private async Task EliminarProducto(ProductoDto producto)
        {
            if (producto == null) return;

            if (MessageBox.Show($"¿Borrar '{producto.Nombre}' permanentemente?", "Confirmar Eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                await _productoService.DeleteAsync(producto.Id);

                // Opción rápida: Lo sacamos de la lista visual sin ir a la BD (más rápido)
                Productos.Remove(producto);
                _listaCompletaRespaldo.Remove(producto);
            }
        }

        [RelayCommand]
        private async Task IrAMaestros()
        {
            // Crear la ventana contenedora
            var window = new Window
            {
                Title = "Gestión de Maestros",
                Height = 500,
                Width = 850,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,

                // Aquí instanciamos la Vista y le asignamos su ViewModel
                Content = new Views.GestionMaestrosView
                {
                    DataContext = new GestionMaestrosViewModel()
                }
            };

            window.ShowDialog(); // Esperar a que cierre

            // Al volver, recargar los datos del inventario (por si cambiaron nombres de categorías)
            await CargarDatos();
        }

        [RelayCommand]
        private void ExportarExcel()
        {
            try
            {
                // 1. Preguntar al usuario dónde quiere guardar el archivo
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel|*.xlsx",
                    FileName = $"Inventario_{DateTime.Now:yyyy-MM-dd}.xlsx",
                    Title = "Guardar Reporte de Inventario"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // 2. Crear el libro de Excel en memoria
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Productos");

                        // --- CABECERAS ---
                        worksheet.Cell(1, 1).Value = "CÓDIGO";
                        worksheet.Cell(1, 2).Value = "PRODUCTO";
                        worksheet.Cell(1, 3).Value = "CATEGORÍA";
                        worksheet.Cell(1, 4).Value = "COSTO";
                        worksheet.Cell(1, 5).Value = "PRECIO VENTA";
                        worksheet.Cell(1, 6).Value = "MARGEN %";
                        worksheet.Cell(1, 7).Value = "STOCK";
                        worksheet.Cell(1, 8).Value = "ESTADO";

                        // Estilo bonito para la cabecera
                        var rangoCabecera = worksheet.Range("A1:H1");
                        rangoCabecera.Style.Font.Bold = true;
                        rangoCabecera.Style.Fill.BackgroundColor = XLColor.FromHtml("#3F51B5"); // Tu color azul
                        rangoCabecera.Style.Font.FontColor = XLColor.White;
                        rangoCabecera.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // --- DATOS ---
                        int fila = 2;
                        // Usamos 'Productos' que es la lista que se ve en pantalla (filtrada o completa)
                        foreach (var p in Productos)
                        {
                            worksheet.Cell(fila, 1).Value = "'" + p.CodigoBarras; // Comilla para que Excel no lo convierta a notación científica
                            worksheet.Cell(fila, 2).Value = p.Nombre;
                            worksheet.Cell(fila, 3).Value = p.Categoria;

                            worksheet.Cell(fila, 4).Value = p.PrecioCompra;
                            worksheet.Cell(fila, 4).Style.NumberFormat.Format = "$ #,##0.00";

                            worksheet.Cell(fila, 5).Value = p.PrecioVenta;
                            worksheet.Cell(fila, 5).Style.NumberFormat.Format = "$ #,##0.00";

                            worksheet.Cell(fila, 6).Value = p.MargenGanancia; // Viene como 0.30
                            worksheet.Cell(fila, 6).Style.NumberFormat.Format = "0.0%"; // Excel lo muestra como 30.0%

                            worksheet.Cell(fila, 7).Value = p.Stock;
                            worksheet.Cell(fila, 8).Value = p.EstadoStock;

                            // Colorear celda de Estado según el texto
                            if (p.EstadoStock == "STOCK BAJO")
                                worksheet.Cell(fila, 8).Style.Font.FontColor = XLColor.Red;
                            else if (p.EstadoStock == "STOCK ALTO")
                                worksheet.Cell(fila, 8).Style.Font.FontColor = XLColor.Green;

                            fila++;
                        }

                        // Ajustar ancho de columnas automáticamente
                        worksheet.Columns().AdjustToContents();

                        // 3. Guardar el archivo físico
                        workbook.SaveAs(saveFileDialog.FileName);

                        MessageBox.Show("¡Inventario exportado exitosamente!", "Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}