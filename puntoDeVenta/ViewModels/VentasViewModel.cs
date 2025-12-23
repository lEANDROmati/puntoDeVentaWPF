using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Entidades;
using Negocio;
using puntoDeVenta.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;


namespace puntoDeVenta.ViewModels
{
    public partial class VentasViewModel : ObservableObject
    {
        private readonly ProductoService _productoService;
        private readonly VentaService _ventaService;

        // El carrito de compras
        [ObservableProperty] private ObservableCollection<DetalleVenta> carrito;

        // Para cuando buscas por nombre y salen muchos
        [ObservableProperty] private ObservableCollection<Producto> resultadosBusqueda;

        // Lo que escribe el cajero
        [ObservableProperty] private string textoBusqueda;

        // Totales
        [ObservableProperty] private decimal totalVenta;
        [ObservableProperty] private int cantidadProductos;

        // Selección
        [ObservableProperty] private Producto productoSeleccionadoBusqueda;
        [ObservableProperty] private DetalleVenta detalleSeleccionadoCarrito;

        public VentasViewModel()
        {
            _productoService = new ProductoService();
            _ventaService = new VentaService(); 

            Carrito = new ObservableCollection<DetalleVenta>();
            ResultadosBusqueda = new ObservableCollection<Producto>();
        }

        // --- COMANDO PRINCIPAL: BUSCAR (Al dar Enter) ---
        [RelayCommand]
        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda)) return;

            ResultadosBusqueda.Clear();

            // 1. Intentar buscar por CÓDIGO EXACTO (Prioridad Máxima)
            var productoPorCodigo = _productoService.GetByCodigo(TextoBusqueda);

            if (productoPorCodigo != null)
            {
                // ¡Encontrado! Agregar directo y limpiar
                AgregarAlCarrito(productoPorCodigo);
                TextoBusqueda = "";
                return;
            }

            // 2. Si no es código, buscar por NOMBRE (Coincidencias)
            var coincidencias = _productoService.Search(TextoBusqueda);

            if (coincidencias.Count == 0)
            {
                MessageBox.Show("Producto no encontrado.");
            }
            else if (coincidencias.Count == 1)
            {
                // Si solo hay uno, agregar directo
                AgregarAlCarrito(coincidencias[0]);
                TextoBusqueda = "";
            }
            else
            {
                // Si hay muchos, mostrarlos en la lista para que el cajero elija
                foreach (var p in coincidencias) ResultadosBusqueda.Add(p);
            }
        }

        // --- COMANDO: AGREGAR AL CARRITO ---
        [RelayCommand]
        private void SeleccionarDeLista()
        {
            if (ProductoSeleccionadoBusqueda != null)
            {
                AgregarAlCarrito(ProductoSeleccionadoBusqueda);
                ResultadosBusqueda.Clear();
                TextoBusqueda = "";
                ProductoSeleccionadoBusqueda = null;
            }
        }

        private void AgregarAlCarrito(Producto producto)
        {
            // 1. Validar Stock Visual (Evitar vender si está en 0)
            if (producto.Stock <= 0)
            {
                MessageBox.Show($"¡No hay stock de '{producto.Nombre}'!", "Agotado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Descontar Stock VISUALMENTE (En memoria, para que el cajero vea que baja)
            producto.Stock--;

            // 3. Buscar si ya existe en el carrito
            var existente = Carrito.FirstOrDefault(d => d.ProductoId == producto.Id);

            if (existente != null)
            {
                // A) SI YA EXISTE: Sumamos cantidad
                existente.Cantidad++;
                existente.Subtotal = existente.PrecioUnitario * existente.Cantidad;

                // --- CORRECCIÓN CLAVE PARA LA LISTA ---
                // Para que la tabla se refresque, quitamos e insertamos en la misma posición.
                // Esto fuerza a la UI a redibujar la fila.
                int index = Carrito.IndexOf(existente);
                Carrito.RemoveAt(index);
                Carrito.Insert(index, existente);

                // Seleccionamos la línea modificada para que se vea
                DetalleSeleccionadoCarrito = existente;
            }
            else
            {
                // B) SI ES NUEVO: Creamos la línea
                var nuevoDetalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Producto = producto,
                    PrecioUnitario = producto.PrecioVenta,
                    Cantidad = 1,
                    Subtotal = producto.PrecioVenta
                };
                Carrito.Add(nuevoDetalle);

                // Scroll automático al último (opcional)
                DetalleSeleccionadoCarrito = nuevoDetalle;
            }

            RecalcularTotal();

            // Devolvemos el foco al buscador (Lógica en ViewModel a veces requiere mensajería, 
            // pero por ahora el usuario suele dar Enter y el foco queda ahí).
        }

        // --- COMANDO: QUITAR DEL CARRITO ---
        [RelayCommand]
        private void QuitarDelCarrito()
        {
            // Validación inicial
            if (DetalleSeleccionadoCarrito == null) return;

            // 1. Guardamos el ítem en una variable temporal
            var itemActual = DetalleSeleccionadoCarrito;

            // 2. Devolver el Stock (Visual)
            var productoOriginal = itemActual.Producto;
            if (productoOriginal != null)
            {
                productoOriginal.Stock++;
            }

            // 3. Lógica de restar
            if (itemActual.Cantidad > 1)
            {
                itemActual.Cantidad--;
                itemActual.Subtotal = itemActual.PrecioUnitario * itemActual.Cantidad;

                // --- AQUÍ ESTÁ EL CAMBIO PARA QUE SE REFRESQUE ---
                var index = Carrito.IndexOf(itemActual);

                // En vez de reemplazar, lo sacamos y lo metemos de nuevo.
                // Esto obliga al DataGrid a pintar la fila desde cero con el nuevo número.
                Carrito.RemoveAt(index);       // <--- FUERA
                Carrito.Insert(index, itemActual); // <--- DENTRO DE NUEVO

                // Volvemos a seleccionarlo para que no se pierda el foco azul
                DetalleSeleccionadoCarrito = itemActual;
            }
            else
            {
                // Si queda 1, lo borramos definitivamente
                Carrito.Remove(itemActual);
            }

            RecalcularTotal();
        }

        [RelayCommand]
        private void CancelarVenta()
        {
            if (Carrito.Count > 0)
            {
                if (MessageBox.Show("¿Vaciar carrito?", "Cancelar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Carrito.Clear();
                    RecalcularTotal();
                    ResultadosBusqueda.Clear();
                    TextoBusqueda = "";
                }
            }
        }

        // --- AUXILIARES ---
        private void RecalcularTotal()
        {
            TotalVenta = Carrito.Sum(x => x.Subtotal);
            CantidadProductos = Carrito.Sum(x => x.Cantidad);
        }


        [RelayCommand]
        private void Cobrar()
        {
            if (Carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.");
                return;
            }

            // 1. Abrir ventana de cobro
            var ventanaCobro = new CobrarWindow(TotalVenta);
            bool? resultado = ventanaCobro.ShowDialog();

            if (resultado == true)
            {
                try
                {
                    // 2. Obtener datos de la ventana
                    var detallesLista = Carrito.ToList();
                    decimal importe = ventanaCobro.PagoRealizado;
                    string metodo = ventanaCobro.MetodoPagoSeleccionado;

                    // 3. ENVIAR AL SERVICIO (Con los nuevos datos)
                    _ventaService.GuardarVenta(TotalVenta, detallesLista, importe, metodo);

                    // 4. Mensaje de Éxito
                    decimal vuelto = importe - TotalVenta;
                    MessageBox.Show($"¡Venta registrada con {metodo}!\nVuelto: ${vuelto:N2}", "Venta Exitosa");

                   
                    // 5. Limpieza
                    Carrito.Clear();
                    RecalcularTotal();
                    TextoBusqueda = "";
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error al guardar la venta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}