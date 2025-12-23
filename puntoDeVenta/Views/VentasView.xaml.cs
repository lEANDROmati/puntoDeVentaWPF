using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;


namespace puntoDeVenta.Views
{
    public partial class VentasView : UserControl
    {
        public VentasView()
        {
           InitializeComponent();
           this.Loaded += VentasView_Loaded;

            WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
            {
                if (m == "VentaFinalizada")
                {
                    // Usamos Dispatcher para asegurar que ocurra en el hilo visual
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        EnfocarBuscador();
                    });
                }
            });
        }
        private void VentasView_Loaded(object sender, RoutedEventArgs e)
        {
            // Forzamos el foco en la caja de texto y seleccionamos todo por si hay texto viejo
            txtBuscar.Focus();
            txtBuscar.SelectAll();
        }

        // 1. Al cargar la pantalla, foco en el Buscador
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EnfocarBuscador();
        }

        // Métodos auxiliares para mover el foco
        public void EnfocarBuscador()
        {
            txtBuscar.Focus();
            txtBuscar.SelectAll();
        }

        

        // Escuchamos los cambios del teclado
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            // Si presionan F5 -> Foco al Buscador
            if (e.Key == Key.F5)
            {
                EnfocarBuscador();
                e.Handled = true;
            }

            
        }
    }
}
