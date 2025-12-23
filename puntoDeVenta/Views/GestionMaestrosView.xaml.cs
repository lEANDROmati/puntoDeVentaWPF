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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace puntoDeVenta.Views
{
    public partial class GestionMaestrosView : UserControl
    {
        // Instanciamos los servicios que creamos hoy
        private readonly CategoriaService _categoriaService = new CategoriaService();
        private readonly UnidadMedidaService _unidadService = new UnidadMedidaService();

        public GestionMaestrosView()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Llenamos las grillas consultando a la BD
                gridCategorias.ItemsSource = _categoriaService.GetActivas();
                gridUnidades.ItemsSource = _unidadService.GetActivas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
        }

        // --- LÓGICA CATEGORÍAS ---

        private void BtnGuardarCat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nuevaCat = new Categoria
                {
                    Nombre = txtNombreCat.Text,
                    Activo = true
                };

                _categoriaService.Guardar(nuevaCat);

                // Limpiar y Recargar
                txtNombreCat.Clear();
                CargarDatos();
                MessageBox.Show("Categoría guardada.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEliminarCat_Click(object sender, RoutedEventArgs e)
        {
            // Truco para obtener el objeto de la fila donde se hizo click
            if (sender is Button btn && btn.DataContext is Categoria cat)
            {
                if (MessageBox.Show($"¿Eliminar '{cat.Nombre}'?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _categoriaService.Eliminar(cat.Id);
                    CargarDatos();
                }
            }
        }

        // --- LÓGICA UNIDADES ---

        private void BtnGuardarUni_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nuevaUni = new UnidadMedida
                {
                    Nombre = txtNombreUni.Text,
                    Abreviatura = txtAbreviaUni.Text,
                    Activo = true
                };

                _unidadService.Guardar(nuevaUni);

                txtNombreUni.Clear();
                txtAbreviaUni.Clear();
                CargarDatos();
                MessageBox.Show("Unidad guardada.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEliminarUni_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UnidadMedida uni)
            {
                if (MessageBox.Show($"¿Eliminar '{uni.Nombre}'?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _unidadService.Eliminar(uni.Id);
                    CargarDatos();
                }
            }
        }
    }
}
