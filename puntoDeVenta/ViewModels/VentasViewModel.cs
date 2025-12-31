    using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Entidades;
using Negocio;
using puntoDeVenta.Services;
using puntoDeVenta.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;


namespace puntoDeVenta.ViewModels
{
    public partial class VentasViewModel : ObservableObject
    {
        // --- SERVICIOS ---
        private readonly ProductoService _productoService;
        private readonly VentaService _ventaService;
        private readonly TicketService _ticketService;
        private readonly ConfigService _configService;
        private readonly CajaService _cajaService;
        private readonly ImpresionService _impresionService;

        // --- PROPIEDADES OBSERVABLES (Aquí están todas las que necesitas) ---

        // 1. El Carrito ahora usa "ItemCarrito" (la clase visual), NO "DetalleVenta"
        [ObservableProperty]
        private ObservableCollection<ItemCarrito> carrito;

        // 2. El Total de la venta ($)
        [ObservableProperty]
        private decimal totalVenta;

        // 3. El item seleccionado en la grilla (para poder borrarlo con SUPR)
        [ObservableProperty]
        private ItemCarrito itemSeleccionado;

        // 4. Texto del Buscador (Manual, para la lógica "Scan & Go")
        private string textoBusqueda;
        public string TextoBusqueda
        {
            get => textoBusqueda;
            set
            {
                if (SetProperty(ref textoBusqueda, value))
                {
                    if (string.IsNullOrEmpty(value)) return;

                    // CASO 1: MULTIPLICADOR (Ej: "6*779123")
                    if (value.Contains("*"))
                    {
                        var partes = value.Split('*');
                        // Verificamos: [numero] * [algo]
                        if (partes.Length == 2 && int.TryParse(partes[0], out int cantidad) && cantidad > 0)
                        {
                            string codigo = partes[1];
                            // Buscamos el producto por el código de la segunda parte
                            var producto = _productoService.GetByCodigo(codigo);

                            if (producto != null)
                            {
                                AgregarAlCarrito(producto, cantidad); // ¡Agregamos con cantidad!

                                textoBusqueda = ""; // Limpiamos
                                OnPropertyChanged(nameof(TextoBusqueda));
                                return; // Salimos para no evaluar lo de abajo
                            }
                        }
                    }

                    // CASO 2: SOLO NÚMEROS (Scan & Go normal)
                    if (value.All(char.IsDigit))
                    {
                        var producto = _productoService.GetByCodigo(value);
                        if (producto != null)
                        {
                            AgregarAlCarrito(producto, 1); // Cantidad 1 por defecto

                            textoBusqueda = "";
                            OnPropertyChanged(nameof(TextoBusqueda));
                        }
                    }
                }
            }
        }

        // --- CONSTRUCTOR ---
        public VentasViewModel()
        {
            _productoService = new ProductoService();
            _ventaService = new VentaService();
            _ticketService = new TicketService();
            _configService = new ConfigService();
            _cajaService = new CajaService();
            _impresionService = new ImpresionService();

            Carrito = new ObservableCollection<ItemCarrito>();
        }

        // --- MÉTODO 1: AGREGAR AL CARRITO (Con lógica de edición) ---
        private void AgregarAlCarrito(Producto p, int cantidadInicial = 1)
        {
            // Buscamos si ya existe en el carrito visual
            var existente = Carrito.FirstOrDefault(d => d.Producto.Id == p.Id);

            if (existente != null)
            {
                // Si existe, sumamos 1. 
                // Como ItemCarrito es Observable, la vista se actualiza sola.
                existente.Cantidad += cantidadInicial;
            }
            else
            {
                // Si no existe, creamos el "Item Visual"
                var nuevoItem = new ItemCarrito
                {
                    Producto = p,
                    PrecioUnitario = p.PrecioVenta,
                    Cantidad = cantidadInicial
                };

                // TRUCO: Nos suscribimos a cambios en ESTE item.
                // Si el usuario cambia la cantidad en la tabla, recalculamos el Total general.
                nuevoItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "Cantidad" || e.PropertyName == "Subtotal")
                    {
                        RecalcularTotal();
                    }
                };

                Carrito.Add(nuevoItem);
            }

            RecalcularTotal();
        }

        // Método auxiliar para sumar todo
        public void RecalcularTotal()
        {
            TotalVenta = Carrito.Sum(d => d.Subtotal);
        }

        // --- MÉTODO 2: COBRAR (Convertir y Guardar) ---
        [RelayCommand]
        private void Cobrar()
        {
            // 1. Validar carrito
            if (Carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.");
                return;
            }

            // 2. Cargar configuración actualizada (para ver si activó/desactivó ticket)
            var config = _configService.ObtenerConfig();
            var cajaAbierta = _cajaService.ObtenerCajaAbierta();

            // 3. Control de Caja (Tu lógica original intacta)
            if (config.UsarControlCaja && cajaAbierta == null)
            {
                if (MessageBox.Show("La caja está cerrada. ¿Deseas abrirla?", "Caja Cerrada", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    new Views.AperturaCajaWindow().ShowDialog();
                    cajaAbierta = _cajaService.ObtenerCajaAbierta();
                    if (cajaAbierta == null) return;
                }
                else
                {
                    return;
                }
            }

            // 4. Abrir ventana de cobro
            var ventanaCobro = new Views.CobrarWindow(TotalVenta);
            if (ventanaCobro.ShowDialog() == true)
            {
                try
                {
                    // --- CONVERSIÓN ---
                    var detallesParaGuardar = Carrito.Select(item => new DetalleVenta
                    {
                        ProductoId = item.Producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = item.Cantidad * item.PrecioUnitario
                    }).ToList();

                    decimal importe = ventanaCobro.PagoRealizado;
                    string metodo = ventanaCobro.MetodoPagoSeleccionado;
                    decimal vuelto = importe - TotalVenta;

                    // 5. GUARDAR VENTA
                    int idVenta = _ventaService.GuardarVenta(TotalVenta, detallesParaGuardar, importe, metodo);

                    // ============================================================
                    // 6. IMPRESIÓN AUTOMÁTICA (Lógica Nueva)
                    // ============================================================
                    if (config.ImprimirTicket)
                    {
                        // Creamos un objeto venta temporal solo para el ticket
                        var ventaTicket = new Venta { Id = idVenta, Total = TotalVenta };

                        // Llamamos al servicio pasando la CONFIGURACIÓN (que tiene la impresora)
                        _impresionService.ImprimirTicket(
                            ventaTicket,
                            detallesParaGuardar,
                            importe,
                            vuelto,
                            config // <--- Pasamos config aquí
                        );
                    }
                    // ============================================================

                    // 7. Limpiar y terminar
                    Carrito.Clear();
                    RecalcularTotal();
                    TextoBusqueda = "";

                    // Opcional: Feedback sutil en barra de estado en vez de MessageBox intrusivo
                    // StatusMessage = "Venta registrada con éxito"; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error crítico al cobrar: {ex.Message}");
                }
            }
        }

        // --- OTROS COMANDOS NECESARIOS ---

        [RelayCommand]
        private void BuscarProducto()
        {
            // Lógica manual cuando presionan Enter en texto (no numérico)
            if (string.IsNullOrWhiteSpace(TextoBusqueda)) return;

            // (Aquí iría tu lógica de búsqueda por nombre)
            // ...
        }

        [RelayCommand]
        private void EliminarItem(ItemCarrito item)
        {
            // 1. Si presionas SUPR, 'item' es null. Usamos el seleccionado.
            if (item == null) item = ItemSeleccionado;

            // 2. Si venía del botón X, 'item' ya trae el producto.

            if (item != null)
            {
                Carrito.Remove(item);
                RecalcularTotal();

                // (Opcional) Limpiamos la selección para evitar errores
                ItemSeleccionado = null;
            }
        }

        [RelayCommand]
        private void CancelarVenta()
        {
            if (Carrito.Count > 0 && MessageBox.Show("¿Cancelar?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Carrito.Clear();
                RecalcularTotal();
                TextoBusqueda = "";
            }
        }

        [RelayCommand]
        private void AbrirBuscador()
        {
            var buscador = new BuscadorProductosWindow();
            bool? resultado = buscador.ShowDialog();

            // Si seleccionó algo...
            if (resultado == true && buscador.ProductoSeleccionado != null)
            {
                var dto = buscador.ProductoSeleccionado;

                // CONVERSIÓN: De DTO a Entidad (para el carrito)
                var productoEntidad = new Producto
                {
                    Id = dto.Id,
                    Nombre = dto.Nombre,
                    // CORRECCIÓN: Asignamos CodigoBarras
                    CodigoBarras = dto.CodigoBarras,
                    PrecioVenta = dto.PrecioVenta,
                    Stock = dto.Stock,
                    // (Si tu entidad tiene CategoriaId u otros campos obligatorios, 
                    // el carrito visual quizás no los necesite para mostrar precio y nombre)
                };

                AgregarAlCarrito(productoEntidad);

                TextoBusqueda = "";
            }
        }
    }
}