using Negocio;
using System.Windows;

namespace puntoDeVenta.Views
{
    public partial class AperturaCajaWindow : Window
    {
        private readonly CajaService _cajaService = new CajaService();
        public decimal MontoIngresado { get; private set; }

        public AperturaCajaWindow()
        {
            InitializeComponent();

            // TRUCO PRO: Al cargar, pone el foco y selecciona el "0"
            // Así el usuario solo escribe el número y listo.
            Loaded += (s, e) =>
            {
                txtMonto.Focus();
                txtMonto.SelectAll();
            };
        }

        private async void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal monto))
            {
                try
                {
                    // --- PASO CRÍTICO QUE FALTABA ---
                    // Guardamos en la Base de Datos
                    await _cajaService.AbrirCajaAsync(monto);
                    // --------------------------------

                    MontoIngresado = monto;
                    this.DialogResult = true; // Cierra y avisa que todo salió bien
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir la caja: {ex.Message}");
                    // No cerramos la ventana para que pueda intentar de nuevo
                }
            }
            else
            {
                MessageBox.Show("Monto inválido");
                txtMonto.SelectAll();
                txtMonto.Focus();
            }
        }
    }
}