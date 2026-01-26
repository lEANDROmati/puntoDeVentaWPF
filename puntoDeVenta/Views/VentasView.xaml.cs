using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace puntoDeVenta.Views
{
   public partial class VentasView : UserControl
    {
        public VentasView()
        {
            InitializeComponent();

           
            this.Loaded += (s, e) =>
            {
                PonerFoco();
                SuscribirScrollAutomatico();
            };

           
            this.PreviewKeyDown += VentasView_PreviewKeyDown;
        }

        private void PonerFoco()
        {
            txtBuscar.Focus();
            txtBuscar.SelectAll();
        }

        private void SuscribirScrollAutomatico()
        {
           
            if (this.DataContext is ViewModels.VentasViewModel vm)
            {
                
                vm.Carrito.CollectionChanged += (s, e) =>
                {
                   
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                       
                        if (vm.Carrito.Count > 0)
                        {
                            var ultimoItem = vm.Carrito[vm.Carrito.Count - 1];
                            gridCarrito.ScrollIntoView(ultimoItem);
                        }
                    }
                };
            }
        }

        private void VentasView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Key == Key.F1)
            {
                PonerFoco();
                e.Handled = true;
            }

           
            if (e.Key == Key.Delete)
            {
               
                if (this.DataContext is ViewModels.VentasViewModel vm)
                {
                   
                    if (vm.EliminarItemCommand.CanExecute(null))
                    {
                        vm.EliminarItemCommand.Execute(null);

                       
                        PonerFoco();

                        e.Handled = true; 
                    }
                }
            }
        }
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            
            PonerFoco();
        }
    }
}
