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

        // CORRECCIÓN 1: Usamos la lista de DTOs, no de Entidades
        private List<ProductoDto> _todosLosProductos;

        // CORRECCIÓN 2: Lo que seleccionas es un DTO
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
            // Ahora sí coinciden los tipos (List<ProductoDto>)
            _todosLosProductos = await _productoService.GetAllAsync();
            gridProductos.ItemsSource = _todosLosProductos;
        }

        private void TxtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtFiltro.Text.ToLower();

            if (_todosLosProductos == null) return;

            // CORRECCIÓN 3: Filtramos usando "CodigoBarras" (tu nombre real)
            var filtrados = _todosLosProductos.Where(p =>
                (p.Nombre != null && p.Nombre.ToLower().Contains(filtro)) ||
                // Usamos ?. por si es null, y "CodigoBarras"
                (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(filtro))
            ).ToList();

            gridProductos.ItemsSource = filtrados;
        }

        private void ConfirmarSeleccion()
        {
            // Casteamos a ProductoDto
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