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
            CargarDatos();
        }

        public void CargarDatos()
        {
            try
            {
                // 1. El servicio devuelve una lista de DTOs (Datos ya procesados)
                // p.Categoria ya es un string (ej: "Bebidas")
                // p.Unidad ya es un string (ej: "lt")
                var productosDto = _productoService.GetAll();

                var listaTemporal = new List<ProductoDto>();

                foreach (var p in productosDto)
                {
                    // Recalculamos estado y margen solo para asegurarnos que la visual esté fresca,
                    // aunque el servicio ya suele traerlo.
                    string estado = CalcularEstado(p.Stock, p.StockMinimo);

                    // Protección contra división por cero
                    decimal margen = (p.PrecioVenta == 0) ? 0 : (p.PrecioVenta - p.PrecioCompra) / p.PrecioVenta;

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
        private void NuevoProducto()
        {
            var ventana = new Views.ProductoFormWindow(); // Asegúrate del namespace correcto
            if (ventana.ShowDialog() == true)
            {
                CargarDatos();
            }
        }

        // CAMBIO CLAVE: Recibimos el producto como parámetro desde el botón de la fila
        [RelayCommand]
        private void EditarProducto(ProductoDto producto)
        {
            if (producto == null) return;

            // Buscamos el producto REAL en la BD
            var productoReal = _productoService.GetById(producto.Id);

            if (productoReal != null)
            {
                var ventana = new Views.ProductoFormWindow(productoReal);
                if (ventana.ShowDialog() == true)
                {
                    CargarDatos(); // Recargamos para ver los cambios
                    TextoBusqueda = ""; // Limpiamos buscador opcionalmente
                }
            }
        }

        // CAMBIO CLAVE: Recibimos el parámetro
        [RelayCommand]
        private void EliminarProducto(ProductoDto producto)
        {
            if (producto == null) return;

            if (MessageBox.Show($"¿Borrar '{producto.Nombre}' permanentemente?", "Confirmar Eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _productoService.Delete(producto.Id);

                // Opción rápida: Lo sacamos de la lista visual sin ir a la BD (más rápido)
                Productos.Remove(producto);
                _listaCompletaRespaldo.Remove(producto);
            }
        }

        [RelayCommand]
        private void IrAMaestros()
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
            CargarDatos();
        }
        [RelayCommand]
        private void ExportarExcel()
        {
            MessageBox.Show("Próximamente: Exportar a Excel");
        }
    }
}