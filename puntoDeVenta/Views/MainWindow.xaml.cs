using puntoDeVenta.Services;
using System.Windows;
using System.Windows.Input;

namespace puntoDeVenta.Views
{
    public partial class MainWindow : Window
    {
        private Rect _posicionAnterior;
        private bool _esMaximizado = false;
        public MainWindow()
        {
            InitializeComponent();
            _posicionAnterior = new Rect(100, 100, 1000, 700);
        }

        
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                
                this.DragMove();
            }
        }

        // 2. BOTÓN MINIMIZAR
        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 3. BOTÓN MAXIMIZAR / RESTAURAR
        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            if (_esMaximizado)
            {
                // RESTAURAR (Volver al tamaño normal)
                this.Left = _posicionAnterior.Left;
                this.Top = _posicionAnterior.Top;
                this.Width = _posicionAnterior.Width;
                this.Height = _posicionAnterior.Height;

                _esMaximizado = false;
            }
            else
            {
                // GUARDAR (Recordar dónde estaba antes de agrandar)
                _posicionAnterior = new Rect(this.Left, this.Top, this.Width, this.Height);

                // MAXIMIZAR MANUAL (Ocupar el área de trabajo exacta)
                // SystemParameters.WorkArea te da el tamaño de la pantalla MENOS la barra de tareas.
                this.Left = SystemParameters.WorkArea.Left;
                this.Top = SystemParameters.WorkArea.Top;
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;

                _esMaximizado = true;
            }
        }

        // 4. BOTÓN CERRAR (X) -> Cierra toda la app
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // 5. CERRAR SESIÓN (Botón del menú lateral)
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Seguro que deseas cerrar sesión?", "Salir",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Abrir Login nuevamente
                var login = new LoginWindow();
                login.Show();

                // Cerrar esta ventana principal
                this.Close();
            }
        }
    }
}