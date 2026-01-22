using Entidades;
using Negocio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace puntoDeVenta.Views
{
    public partial class ProductoFormWindow : Window
    {
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;
        private readonly UnidadMedidaService _unidadService;

        private Producto _productoActual;

        // Bandera para evitar bucles infinitos en el cálculo automático
        private bool _isCalculating = false;
        private bool _cargandoDatos = false;

        // Constructor para NUEVO
        public ProductoFormWindow()
        {
            InitializeComponent();
            _productoService = new ProductoService();
            _categoriaService = new CategoriaService();
            _unidadService = new UnidadMedidaService();

            _productoActual = new Producto();
            CargarCombos();             // Async fire-and-forget
            ConectarEventosCalculadora(); // <--- Conectamos la magia
        }

        // Constructor para EDITAR
        public ProductoFormWindow(Producto productoAEditar) : this() // llama al constructor sin parametros primero
        {
            _productoActual = productoAEditar;
            this.DataContext = _productoActual;
            // Se sobreescribe lo de CargarCombos que se ejecutó en this() ???
            // Ojo: CargarCombos es async ahora.
            // Mejor esperamos CargarCombos? No podemos en ctor.
            // CargarDatosEnPantalla solo setea textos.
            CargarDatosEnPantalla();
        }

        private void ConectarEventosCalculadora()
        {
            // Lógica 1: Si cambio Costo o Margen -> Calculo Precio
            txtCosto.TextChanged += (s, e) => CalcularPrecioVenta();
            txtMargen.TextChanged += (s, e) => CalcularPrecioVenta();

            // Lógica 2: Si cambio Precio -> Calculo Margen (Inverso)
            txtPrecioVenta.TextChanged += (s, e) => CalcularMargen();

            // Eventos de botones
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += (s, e) => this.Close();
            btnGenerarCodigo.Click += BtnGenerarCodigo_Click;
        }

        private void CalcularMargen()
        {
            // Si estamos cargando o calculando el precio, ALTO.
            if (_cargandoDatos || _isCalculating) return;

            try
            {
                _isCalculating = true; // 🔴 Bloqueo

                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta);

                if (costo > 0)
                {
                    // Fórmula Inversa: ((Precio - Costo) / Costo) * 100
                    decimal margen = ((precioVenta - costo) / costo) * 100;
                    txtMargen.Text = Math.Round(margen, 2).ToString("N2");
                }
                else
                {
                    // Si no hay costo, no podemos calcular margen matemático real
                    txtMargen.Text = "0";
                }
            }
            finally
            {
                _isCalculating = false; // 🟢 Desbloqueo
            }
        }

        private void CargarDatosEnPantalla()
        {
            _cargandoDatos = true; // 1. Bloqueamos

            try 
            {
                txtCodigo.Text = _productoActual.CodigoBarras;
                txtNombre.Text = _productoActual.Nombre;
                txtCosto.Text = _productoActual.PrecioCompra.ToString("N2");
                txtPrecioVenta.Text = _productoActual.PrecioVenta.ToString("N2");
                txtStock.Text = _productoActual.Stock.ToString();
                txtMinimo.Text = _productoActual.StockMinimo.ToString();

                // Calcular el Margen visualmente
                if (_productoActual.PrecioCompra > 0)
                {
                    decimal margen = ((_productoActual.PrecioVenta - _productoActual.PrecioCompra) / _productoActual.PrecioCompra) * 100;
                    txtMargen.Text = Math.Round(margen, 2).ToString();
                }
                else
                {
                    txtMargen.Text = "0";
                }

                cmbCategoria.SelectedValue = _productoActual.CategoriaId;
                cmbUnidad.SelectedValue = _productoActual.UnidadMedidaId;
            }
            finally 
            {
                _cargandoDatos = false; 
            }
        }

        private async void CargarCombos()
        {
            var cats = await _categoriaService.GetActivasAsync();
            var units = await _unidadService.GetActivasAsync();

            cmbCategoria.ItemsSource = cats;
            cmbUnidad.ItemsSource = units;

            cmbCategoria.SelectedIndex = 0;
            cmbUnidad.SelectedIndex = 0;
            
            // Si estábamos en modo editar, recargamos la selección
            if (_productoActual != null && _productoActual.Id > 0)
            {
                 cmbCategoria.SelectedValue = _productoActual.CategoriaId;
                 cmbUnidad.SelectedValue = _productoActual.UnidadMedidaId;
            }
        }

        // --- LÓGICA DE CALCULADORA (COSTO + MARGEN = PRECIO) ---
        private void CalcularPrecioVenta()
        {
            // Si estamos cargando datos o si YA estamos calculando el inverso, ALTO.
            if (_cargandoDatos || _isCalculating) return;

            try
            {
                _isCalculating = true; // 🔴 Bloqueo (Semáforo en Rojo)

                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtMargen.Text, out decimal margen);

                // Fórmula: Costo * (1 + Margen/100)
                decimal precioFinal = costo * (1 + (margen / 100));

                // Actualizamos Precio (esto disparará TextChanged de Precio, pero el semáforo lo frenará)
                txtPrecioVenta.Text = Math.Round(precioFinal, 2).ToString("N2");
            }
            finally
            {
                _isCalculating = false; // 🟢 Desbloqueo (Semáforo en Verde)
            }
        }

        // --- GENERADOR DE CÓDIGO ---
        private void BtnGenerarCodigo_Click(object sender, RoutedEventArgs e)
        {
            // Genera un número aleatorio de 8 dígitos para productos caseros
            var random = new Random();
            string codigo = random.Next(10000000, 99999999).ToString();
            txtCodigo.Text = codigo;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones Básicas
                if (string.IsNullOrWhiteSpace(txtNombre.Text)) throw new Exception("El nombre es obligatorio.");
                if (string.IsNullOrWhiteSpace(txtPrecioVenta.Text)) throw new Exception("El precio es obligatorio.");

                // Mapeo de Datos
                _productoActual.CodigoBarras = txtCodigo.Text;
                _productoActual.Nombre = txtNombre.Text;

                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtPrecioVenta.Text, out decimal precio); // Ojo: txtPrecioVenta
                int.TryParse(txtStock.Text, out int stock);
                int.TryParse(txtMinimo.Text, out int stockMin); // Ojo: txtMinimo

                _productoActual.PrecioCompra = costo;
                _productoActual.PrecioVenta = precio;
                _productoActual.Stock = stock;
                _productoActual.StockMinimo = stockMin;

                // Valores por defecto (ya que los quitamos del XAML para limpiar)
                _productoActual.Activo = true;
                _productoActual.ControlarStock = true;
                _productoActual.TieneIVA = false;

                if (cmbCategoria.SelectedValue != null)
                    _productoActual.CategoriaId = (int)cmbCategoria.SelectedValue;

                if (cmbUnidad.SelectedValue != null)
                    _productoActual.UnidadMedidaId = (int)cmbUnidad.SelectedValue;

                // Guardar
                await _productoService.GuardarAsync(_productoActual);

                MessageBox.Show("Producto guardado con éxito.", "Inventario", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
