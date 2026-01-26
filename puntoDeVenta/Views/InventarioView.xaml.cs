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
   
    public partial class InventarioView : UserControl
    {
        public InventarioView()
        {
            InitializeComponent();
            this.Loaded += InventarioView_Loaded;
        }
        private void InventarioView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
          
            if (this.DataContext is InventarioViewModel viewModel)
            {
               
                viewModel.CargarDatos();
            }
        }
    }
}
