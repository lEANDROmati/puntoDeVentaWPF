using puntoDeVenta.ViewModels;
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
    /// <summary>
    /// Lógica de interacción para InventarioView.xaml
    /// </summary>
    public partial class InventarioView : UserControl
    {
        public InventarioView()
        {
            InitializeComponent();
            this.Loaded += InventarioView_Loaded;
        }
        private void InventarioView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Obtenemos el cerebro (ViewModel) que está conectado a esta vista
            if (this.DataContext is InventarioViewModel viewModel)
            {
                // ¡Le ordenamos recargar los datos de la BD!
                viewModel.CargarDatos();
            }
        }
    }
}
