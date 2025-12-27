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

            // 1. Al cargar, poner el foco en el buscador
            this.Loaded += (s, e) =>
            {
                PonerFoco();
                SuscribirScrollAutomatico(); // <--- Llamamos a la nueva función
            };

            // 2. Escuchar teclas (F1)
            this.PreviewKeyDown += VentasView_PreviewKeyDown;
        }

        private void PonerFoco()
        {
            txtBuscar.Focus();
            txtBuscar.SelectAll();
        }

        private void SuscribirScrollAutomatico()
        {
            // Obtenemos el ViewModel
            if (this.DataContext is ViewModels.VentasViewModel vm)
            {
                // Nos suscribimos al evento "CollectionChanged" del Carrito
                vm.Carrito.CollectionChanged += (s, e) =>
                {
                    // Si hubo una acción de "Agregar" (Add)
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        // Scroll al último ítem
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
            // CASO 1: Tecla F1 -> Poner foco
            if (e.Key == Key.F1)
            {
                PonerFoco();
                e.Handled = true;
            }

            // CASO 2: Tecla SUPR (Delete) -> Eliminar item seleccionado
            if (e.Key == Key.Delete)
            {
                // 1. Obtenemos el ViewModel (el cerebro de la pantalla)
                if (this.DataContext is ViewModels.VentasViewModel vm)
                {
                    // 2. Ejecutamos el comando de eliminar (sin parámetros, para que borre el seleccionado)
                    if (vm.EliminarItemCommand.CanExecute(null))
                    {
                        vm.EliminarItemCommand.Execute(null);

                        // 3. ¡Truco! Devolvemos el foco al buscador inmediatamente
                        PonerFoco();

                        e.Handled = true; // Decimos "ya manejé la tecla, nadie más la toque"
                    }
                }
            }
        }
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // Cuando hacen clic en Eliminar, esperamos un microsegundo 
            // a que el botón haga su trabajo y luego robamos el foco de vuelta.
            PonerFoco();
        }
    }
}
