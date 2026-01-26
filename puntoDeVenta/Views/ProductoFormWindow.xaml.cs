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

       
        private bool _isCalculating = false;
        private bool _cargandoDatos = false;

       
        public ProductoFormWindow()
        {
            InitializeComponent();
            _productoService = new ProductoService();
            _categoriaService = new CategoriaService();
            _unidadService = new UnidadMedidaService();

            _productoActual = new Producto();
            CargarCombos();             
            ConectarEventosCalculadora(); 
        }

       
        public ProductoFormWindow(Producto productoAEditar) : this() 
        {
            _productoActual = productoAEditar;
            this.DataContext = _productoActual;
            
            CargarDatosEnPantalla();
        }

        private void ConectarEventosCalculadora()
        {
           
            txtCosto.TextChanged += (s, e) => CalcularPrecioVenta();
            txtMargen.TextChanged += (s, e) => CalcularPrecioVenta();

           
            txtPrecioVenta.TextChanged += (s, e) => CalcularMargen();

            
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += (s, e) => this.Close();
            btnGenerarCodigo.Click += BtnGenerarCodigo_Click;
        }

        private void CalcularMargen()
        {
           
            if (_cargandoDatos || _isCalculating) return;

            try
            {
                _isCalculating = true;

                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtPrecioVenta.Text, out decimal precioVenta);

                if (costo > 0)
                {
                   
                    decimal margen = ((precioVenta - costo) / costo) * 100;
                    txtMargen.Text = Math.Round(margen, 2).ToString("N2");
                }
                else
                {
                   
                    txtMargen.Text = "0";
                }
            }
            finally
            {
                _isCalculating = false; 
            }
        }

        private void CargarDatosEnPantalla()
        {
            _cargandoDatos = true; 

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
            
            
            if (_productoActual != null && _productoActual.Id > 0)
            {
                 cmbCategoria.SelectedValue = _productoActual.CategoriaId;
                 cmbUnidad.SelectedValue = _productoActual.UnidadMedidaId;
            }
        }

       
        private void CalcularPrecioVenta()
        {
            
            if (_cargandoDatos || _isCalculating) return;

            try
            {
                _isCalculating = true; 

                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtMargen.Text, out decimal margen);

                
                decimal precioFinal = costo * (1 + (margen / 100));

                
                txtPrecioVenta.Text = Math.Round(precioFinal, 2).ToString("N2");
            }
            finally
            {
                _isCalculating = false; 
            }
        }

        
        private void BtnGenerarCodigo_Click(object sender, RoutedEventArgs e)
        {
           
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
                decimal.TryParse(txtPrecioVenta.Text, out decimal precio); 
                int.TryParse(txtStock.Text, out int stock);
                int.TryParse(txtMinimo.Text, out int stockMin);

                _productoActual.PrecioCompra = costo;
                _productoActual.PrecioVenta = precio;
                _productoActual.Stock = stock;
                _productoActual.StockMinimo = stockMin;

                
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
