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

        // Constructor para NUEVO
        public ProductoFormWindow()
        {
            InitializeComponent();
            _productoService = new ProductoService();
            _categoriaService = new CategoriaService();
            _unidadService = new UnidadMedidaService();

            _productoActual = new Producto();
            CargarCombos();
            ConectarEventosCalculadora(); // <--- Conectamos la magia
        }

        // Constructor para EDITAR
        public ProductoFormWindow(Producto productoAEditar) : this()
        {
            _productoActual = productoAEditar;
            CargarDatosEnPantalla();
        }

        private void ConectarEventosCalculadora()
        {
            // Suscribimos los eventos manualmente para no ensuciar el XAML
            txtCosto.TextChanged += (s, e) => CalcularPrecioVenta();
            txtMargen.TextChanged += (s, e) => CalcularPrecioVenta();

            // Botones
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += (s, e) => this.Close();
            btnGenerarCodigo.Click += BtnGenerarCodigo_Click;
        }

        private void CargarCombos()
        {
            cmbCategoria.ItemsSource = _categoriaService.GetActivas();
            cmbUnidad.ItemsSource = _unidadService.GetActivas();

            cmbCategoria.SelectedIndex = 0;
            cmbUnidad.SelectedIndex = 0;
        }

        private void CargarDatosEnPantalla()
        {
            txtCodigo.Text = _productoActual.CodigoBarras;
            txtNombre.Text = _productoActual.Nombre;
            txtCosto.Text = _productoActual.PrecioCompra.ToString("N2"); // Formato bonito
            txtPrecioVenta.Text = _productoActual.PrecioVenta.ToString("N2");
            txtStock.Text = _productoActual.Stock.ToString();
            txtMinimo.Text = _productoActual.StockMinimo.ToString();

            // Calcular el Margen visualmente (No se guarda en BD, se calcula)
            // Fórmula: ((Precio - Costo) / Costo) * 100
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

        // --- LÓGICA DE CALCULADORA (COSTO + MARGEN = PRECIO) ---
        private void CalcularPrecioVenta()
        {
            if (_isCalculating) return; // Evita rebote

            try
            {
                _isCalculating = true;

                // Intentamos leer los números. Si están vacíos, usamos 0.
                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtMargen.Text, out decimal margen);

                // Fórmula: Costo * (1 + Margen/100)
                decimal precioFinal = costo * (1 + (margen / 100));

                // Escribimos el resultado en la caja de precio
                txtPrecioVenta.Text = Math.Round(precioFinal, 2).ToString("N2");
            }
            catch
            {
                // Ignorar errores mientras escribe
            }
            finally
            {
                _isCalculating = false;
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

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
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
                _productoService.Guardar(_productoActual);

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