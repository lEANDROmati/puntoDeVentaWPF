using Entidades;
using Negocio;
using Negocio.DTO;
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
    public partial class BuscadorProductosWindow : Window
    {
        private readonly ProductoService _productoService;

        
        private List<ProductoDto> _todosLosProductos;

       
        public ProductoDto ProductoSeleccionado { get; private set; }

        public BuscadorProductosWindow()
        {
            InitializeComponent();
            _productoService = new ProductoService();

            CargarDatos();

            this.Loaded += (s, e) => txtFiltro.Focus();

            gridProductos.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter) ConfirmarSeleccion();
            };
        }

        private async void CargarDatos()
        {
           
            _todosLosProductos = await _productoService.GetAllAsync();
            gridProductos.ItemsSource = _todosLosProductos;
        }

        private void TxtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtFiltro.Text.ToLower();

            if (_todosLosProductos == null) return;

            
            var filtrados = _todosLosProductos.Where(p =>
                (p.Nombre != null && p.Nombre.ToLower().Contains(filtro)) ||
                (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(filtro))
            ).ToList();

            gridProductos.ItemsSource = filtrados;
        }

        private void ConfirmarSeleccion()
        {
           
            if (gridProductos.SelectedItem is ProductoDto p)
            {
                ProductoSeleccionado = p;
                DialogResult = true;
                this.Close();
            }
        }

        private void GridProductos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmarSeleccion();
        }

        private void BtnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            ConfirmarSeleccion();
        }
    }
}