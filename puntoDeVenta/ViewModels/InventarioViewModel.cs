using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entidades;
using Negocio;
using Negocio.DTO; // Asegúrate de tener este using
using puntoDeVenta.Views; // Para abrir las ventanas hijas (ProductoWindow, etc)
using System;
using System.Collections.ObjectModel;
using System.IO; // Para exportar a CSV
using System.Text;
using System.Windows;

namespace puntoDeVenta.ViewModels
{
    public partial class InventarioViewModel : ObservableObject
    {
        // Lista visual de productos (DTO)
        [ObservableProperty]
        private ObservableCollection<ProductoDto> listaProductos;

        // El producto seleccionado en la tabla
        [ObservableProperty]
        private ProductoDto productoSeleccionadoDto;

        public InventarioViewModel()
        {
            ListaProductos = new ObservableCollection<ProductoDto>();

            // NOTA: Quitamos la llamada inicial a CargarDatos() aquí,
            // porque la Vista (InventarioView.xaml.cs) lo llamará con el evento "Loaded".
            // Si no pusiste el código en la Vista, descomenta la siguiente línea:
            // CargarDatos(); 
        }

        // --- MÉTODO PÚBLICO PARA CARGAR DATOS (La Vista lo llama) ---
        public void CargarDatos()
        {
            try
            {
                // CRÍTICO: Creamos el servicio AQUÍ adentro.
                // Esto asegura que la conexión a la BD sea nueva y vea los cambios recientes de la Caja.
                var servicio = new ProductoService();
                var productosDb = servicio.GetAll();

                ListaProductos.Clear();

                foreach (var p in productosDb)
                {
                    ListaProductos.Add(new ProductoDto
                    {
                        Id = p.Id,
                        CodigoBarras = p.CodigoBarras,
                        Nombre = p.Nombre,

                        // Validamos nulos por seguridad (?? "-")
                        Categoria = p.Categoria ?? "-",
                        Unidad = p.Unidad ?? "u.",

                        PrecioCompra = p.PrecioCompra,
                        PrecioVenta = p.PrecioVenta,
                        Stock = p.Stock,
                        StockMinimo = p.StockMinimo, // Importante para el cálculo de estado

                        // Lógica visual
                        EstadoStock = CalcularEstado(p.Stock, p.StockMinimo),
                        MargenGanancia = CalcularMargen(p.PrecioCompra, p.PrecioVenta)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar inventario: {ex.Message}");
            }
        }

        // --- CÁLCULOS AUXILIARES ---
        private string CalcularEstado(int stock, int minimo)
        {
            if (stock <= minimo) return "STOCK BAJO";      // Rojo
            if (stock <= minimo * 3) return "STOCK MEDIO"; // Naranja
            return "STOCK ALTO";                           // Verde
        }

        private decimal CalcularMargen(decimal compra, decimal venta)
        {
            if (venta == 0) return 0;
            return (venta - compra) / venta;
        }

        // --- COMANDOS (BOTONES) ---

        [RelayCommand]
        private void NuevoProducto()
        {
            // Abre la ventana vacía para crear
            var ventana = new ProductoFormWindow();
            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                CargarDatos(); // Si guardó, recargamos la tabla
            }
        }

        [RelayCommand]
        private void EditarProducto()
        {
            if (ProductoSeleccionadoDto != null)
            {
                // 1. Buscamos el producto REAL en la BD (La entidad completa)
                // No podemos pasar el DTO porque le faltan datos internos
                var servicio = new ProductoService();
                var productoReal = servicio.GetById(ProductoSeleccionadoDto.Id);

                if (productoReal != null)
                {
                    // 2. Abrimos la ventana pasándole el producto
                    var ventana = new ProductoFormWindow(productoReal);
                    bool? resultado = ventana.ShowDialog();

                    if (resultado == true)
                    {
                        CargarDatos(); // Si editó, recargamos la tabla
                    }
                }
                else
                {
                    MessageBox.Show("Error: No se encontró el producto en la base de datos.");
                }
            }
            else
            {
                MessageBox.Show("Selecciona un producto de la lista para editar.");
            }
        }

        [RelayCommand]
        private void EliminarProducto()
        {
            if (ProductoSeleccionadoDto != null)
            {
                var confirm = MessageBox.Show($"¿Eliminar '{ProductoSeleccionadoDto.Nombre}'?",
                                              "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        var servicio = new ProductoService();
                        servicio.Delete(ProductoSeleccionadoDto.Id);
                        CargarDatos(); // Recargar tras borrar
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo eliminar: " + ex.Message);
                    }
                }
            }
        }

        [RelayCommand]
        private void ExportarExcel()
        {
            try
            {
                // Exportación simple a CSV
                var sb = new StringBuilder();
                sb.AppendLine("Codigo;Nombre;Categoria;Costo;Venta;Stock;Estado");

                foreach (var item in ListaProductos)
                {
                    sb.AppendLine($"{item.CodigoBarras};{item.Nombre};{item.Categoria};{item.PrecioCompra};{item.PrecioVenta};{item.Stock};{item.EstadoStock}");
                }

                File.WriteAllText("Inventario.csv", sb.ToString());
                MessageBox.Show("Exportado a 'Inventario.csv' correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar: " + ex.Message);
            }
        }

        [RelayCommand]
        private void AbrirMaestros()
        {
            // Aquí puedes poner lógica si tienes ventanas de Categorías/Unidades
            MessageBox.Show("Funcionalidad de Maestros pendiente de implementar ventanas.");
        }
    }
}