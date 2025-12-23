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
        // Lista que se ve en la pantalla
        [ObservableProperty]
        private ObservableCollection<ProductoDto> listaProductos;

        // Producto seleccionado (fila azul)
        [ObservableProperty]
        private ProductoDto productoSeleccionadoDto;

        public InventarioViewModel()
        {
            ListaProductos = new ObservableCollection<ProductoDto>();
            // La carga se hace automática desde la Vista (evento Loaded)
            // para asegurar datos frescos.
        }

        // --- MÉTODO PÚBLICO DE CARGA (El corazón del refresco) ---
        public void CargarDatos()
        {
            try
            {
                // 1. CREAMOS UNA CONEXIÓN NUEVA (Fundamental para ver cambios recientes)
                var servicio = new ProductoService();
                var productosDb = servicio.GetAll();

                ListaProductos.Clear();

                foreach (var p in productosDb)
                {
                    // 2. MAPEO: Pasamos de BD a la Vista
                    ListaProductos.Add(new ProductoDto
                    {
                        Id = p.Id,
                        CodigoBarras = p.CodigoBarras,
                        Nombre = p.Nombre,
                        Categoria = p.Categoria ?? "-", // Evitamos nulos
                        Unidad = p.Unidad ?? "u.",
                        PrecioCompra = p.PrecioCompra,
                        PrecioVenta = p.PrecioVenta,
                        Stock = p.Stock,

                        // IMPORTANTE: Leemos el StockMinimo para poder calcular el estado
                        StockMinimo = p.StockMinimo,

                        // 3. CÁLCULO DEL ESTADO (Aquí estaba el problema antes)
                        EstadoStock = CalcularEstado(p.Stock, p.StockMinimo),

                        // Cálculo del margen
                        MargenGanancia = (p.PrecioVenta == 0) ? 0 : (p.PrecioVenta - p.PrecioCompra) / p.PrecioVenta
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar inventario: {ex.Message}");
            }
        }

        // Lógica de colores/estados
        private string CalcularEstado(int stock, int minimo)
        {
            if (stock <= minimo) return "STOCK BAJO";      // Rojo
            if (stock <= minimo * 3) return "STOCK MEDIO"; // Naranja
            return "STOCK ALTO";                           // Verde
        }

        // --- COMANDOS (BOTONES) ---

        [RelayCommand]
        private void NuevoProducto()
        {
            // Usamos tu ventana de formulario
            var ventana = new ProductoFormWindow();
            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                CargarDatos(); // Si guardó, recargamos lista
            }
        }

        [RelayCommand]
        private void EditarProducto()
        {
            if (ProductoSeleccionadoDto != null)
            {
                // Buscamos el producto REAL en la BD para editarlo completo
                var servicio = new ProductoService();
                var productoReal = servicio.GetById(ProductoSeleccionadoDto.Id);

                if (productoReal != null)
                {
                    var ventana = new ProductoFormWindow(productoReal);
                    bool? resultado = ventana.ShowDialog();

                    if (resultado == true)
                    {
                        CargarDatos(); // Si editó, recargamos
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecciona un producto para editar.");
            }
        }

        [RelayCommand]
        private void EliminarProducto()
        {
            if (ProductoSeleccionadoDto != null)
            {
                if (MessageBox.Show($"¿Borrar '{ProductoSeleccionadoDto.Nombre}'?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var servicio = new ProductoService();
                    servicio.Delete(ProductoSeleccionadoDto.Id);
                    CargarDatos();
                }
            }
        }

        [RelayCommand]
        private void AbrirMaestros()
        {
            // Creamos una ventana vacía al vuelo para alojar tu vista
            var hostWindow = new Window
            {
                Title = "Gestión de Maestros",
                Content = new GestionMaestrosView(), // Metemos tu vista adentro
                Height = 600,
                Width = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            hostWindow.ShowDialog();
            CargarDatos();
        }
    }
}