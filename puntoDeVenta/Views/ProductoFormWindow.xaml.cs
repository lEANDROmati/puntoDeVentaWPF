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

        // Variable para saber si editamos o creamos
        private Producto _productoActual;

        // Constructor para NUEVO
        public ProductoFormWindow()
        {
            InitializeComponent();
            _productoService = new ProductoService();
            _categoriaService = new CategoriaService();
            _unidadService = new UnidadMedidaService();

            _productoActual = new Producto(); // Producto vacío
            CargarCombos();
        }

        // Constructor para EDITAR (recibe un producto)
        public ProductoFormWindow(Producto productoAEditar) : this()
        {
            _productoActual = productoAEditar;
            CargarDatosEnPantalla();
        }

        private void CargarCombos()
        {
            cmbCategoria.ItemsSource = _categoriaService.GetActivas();
            cmbUnidad.ItemsSource = _unidadService.GetActivas();

            // Seleccionar el primero por defecto para ahorrar tiempo
            cmbCategoria.SelectedIndex = 0;
            cmbUnidad.SelectedIndex = 0;
        }

        private void CargarDatosEnPantalla()
        {
            txtCodigo.Text = _productoActual.CodigoBarras;
            txtNombre.Text = _productoActual.Nombre;
            txtCosto.Text = _productoActual.PrecioCompra.ToString();
            txtPrecio.Text = _productoActual.PrecioVenta.ToString();
            txtStock.Text = _productoActual.Stock.ToString();
            txtStockMin.Text = _productoActual.StockMinimo.ToString();
            chkIVA.IsChecked = _productoActual.TieneIVA;
            chkControlarStock.IsChecked = _productoActual.ControlarStock;

            cmbCategoria.SelectedValue = _productoActual.CategoriaId;
            cmbUnidad.SelectedValue = _productoActual.UnidadMedidaId;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Pasar datos de pantalla al objeto
                _productoActual.CodigoBarras = txtCodigo.Text;
                _productoActual.Nombre = txtNombre.Text;

                // Conversiones seguras (si está vacío pone 0)
                decimal.TryParse(txtCosto.Text, out decimal costo);
                decimal.TryParse(txtPrecio.Text, out decimal precio);
                int.TryParse(txtStock.Text, out int stock);
                int.TryParse(txtStockMin.Text, out int stockMin);

                _productoActual.PrecioCompra = costo;
                _productoActual.PrecioVenta = precio;
                _productoActual.Stock = stock;
                _productoActual.StockMinimo = stockMin;

                _productoActual.TieneIVA = chkIVA.IsChecked ?? false;
                _productoActual.ControlarStock = chkControlarStock.IsChecked ?? false;
                _productoActual.Activo = true;

                // Obtener IDs de los Combos
                if (cmbCategoria.SelectedValue != null)
                    _productoActual.CategoriaId = (int)cmbCategoria.SelectedValue;

                if (cmbUnidad.SelectedValue != null)
                    _productoActual.UnidadMedidaId = (int)cmbUnidad.SelectedValue;

                // 2. Enviar al Servicio
                _productoService.Guardar(_productoActual);

                MessageBox.Show("Producto guardado correctamente.");
                this.DialogResult = true; // Cierra la ventana devolviendo éxito
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
