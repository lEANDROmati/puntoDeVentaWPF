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
using System.Threading.Tasks;



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

       

       
        [ObservableProperty]
        private ObservableCollection<ItemCarrito> carrito;

        
        [ObservableProperty]
        private decimal totalVenta;

        
        [ObservableProperty]
        private ItemCarrito itemSeleccionado;

        private string textoBusqueda;
        public string TextoBusqueda
        {
            get => textoBusqueda;
            set
            {
                if (SetProperty(ref textoBusqueda, value))
                {
                    if (string.IsNullOrEmpty(value)) return;
                   
                    ProcesarBusquedaAsync(value);
                }
            }
        }

        private async Task ProcesarBusquedaAsync(string value)
        {
            try
            {
                
                if (value.Contains("*"))
                {
                    var partes = value.Split('*');
                   
                    if (partes.Length == 2 && int.TryParse(partes[0], out int cantidad) && cantidad > 0)
                    {
                        string codigo = partes[1];
                      
                        var producto = await _productoService.GetByCodigoAsync(codigo);

                        if (producto != null)
                        {
                            AgregarAlCarrito(producto, cantidad); 

                            textoBusqueda = ""; 
                            OnPropertyChanged(nameof(TextoBusqueda));
                            return; 
                        }
                    }
                }

                
                if (value.All(char.IsDigit))
                {
                    var producto = await _productoService.GetByCodigoAsync(value);
                    if (producto != null)
                    {
                        AgregarAlCarrito(producto, 1); 

                        textoBusqueda = "";
                        OnPropertyChanged(nameof(TextoBusqueda));
                    }
                }
            }
            catch (System.Exception ex)
            {
               
            }
        }

        
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

        
        private void AgregarAlCarrito(Producto p, int cantidadInicial = 1)
        {
            
            var existente = Carrito.FirstOrDefault(d => d.Producto.Id == p.Id);

            if (existente != null)
            {
               
                existente.Cantidad += cantidadInicial;
            }
            else
            {
                
                var nuevoItem = new ItemCarrito
                {
                    Producto = p,
                    PrecioUnitario = p.PrecioVenta,
                    Cantidad = cantidadInicial
                };

               
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

        
        public void RecalcularTotal()
        {
            TotalVenta = Carrito.Sum(d => d.Subtotal);
        }

       
        [RelayCommand]
        private async Task Cobrar()
        {
            
            if (Carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.");
                return;
            }

            
            var config = await _configService.ObtenerConfigAsync();
            var cajaAbierta = await _cajaService.ObtenerCajaAbiertaAsync();

           
            if (config.UsarControlCaja && cajaAbierta == null)
            {
                if (MessageBox.Show("La caja está cerrada. ¿Deseas abrirla?", "Caja Cerrada", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    new Views.AperturaCajaWindow().ShowDialog();
                    cajaAbierta = await _cajaService.ObtenerCajaAbiertaAsync();
                    if (cajaAbierta == null) return;
                }
                else
                {
                    return;
                }
            }

            //  Abrir ventana de cobro
            var ventanaCobro = new Views.CobrarWindow(TotalVenta);
            if (ventanaCobro.ShowDialog() == true)
            {
                try
                {
                   
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

                    // GUARDAR VENTA
                    int idVenta = await _ventaService.GuardarVentaAsync(TotalVenta, detallesParaGuardar, importe, metodo);

                    
                    // IMPRESIÓN AUTOMÁTICA 
                    
                    if (config.ImprimirTicket)
                    {
                        
                        var ventaTicket = new Venta { Id = idVenta, Total = TotalVenta };

                        
                        _impresionService.ImprimirTicket(
                            ventaTicket,
                            detallesParaGuardar,
                            importe,
                            vuelto,
                            config 
                        );
                    }
                    

                    // Limpiar y terminar
                    Carrito.Clear();
                    RecalcularTotal();
                    TextoBusqueda = "";

                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error crítico al cobrar: {ex.Message}");
                }
            }
        }

        

        [RelayCommand]
        private void BuscarProducto()
        {
           
            if (string.IsNullOrWhiteSpace(TextoBusqueda)) return;

           
        }

        [RelayCommand]
        private void EliminarItem(ItemCarrito item)
        {
           
            if (item == null) item = ItemSeleccionado;

           

            if (item != null)
            {
                Carrito.Remove(item);
                RecalcularTotal();

               
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

            
            if (resultado == true && buscador.ProductoSeleccionado != null)
            {
                var dto = buscador.ProductoSeleccionado;

               
                var productoEntidad = new Producto
                {
                    Id = dto.Id,
                    Nombre = dto.Nombre,
                    CodigoBarras = dto.CodigoBarras,
                    PrecioVenta = dto.PrecioVenta,
                    Stock = dto.Stock,

                };

                AgregarAlCarrito(productoEntidad);

                TextoBusqueda = "";
            }
        }
    }
}