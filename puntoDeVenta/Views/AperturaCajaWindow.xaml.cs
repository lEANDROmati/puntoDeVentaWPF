using System.Windows;

namespace puntoDeVenta.Views
{
    public partial class AperturaCajaWindow : Window
    {
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

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal monto))
            {
                MontoIngresado = monto;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Monto inválido");
                txtMonto.SelectAll(); // Si falla, vuelve a seleccionar para corregir rápido
                txtMonto.Focus();
            }
        }
    }
}