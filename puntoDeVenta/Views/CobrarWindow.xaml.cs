using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace puntoDeVenta.Views
{
    public partial class CobrarWindow : Window
    {
        private decimal _total;

        // Propiedades para que el ViewModel las lea al cerrar
        public decimal PagoRealizado { get; private set; }
        public string MetodoPagoSeleccionado { get; private set; }

        public CobrarWindow(decimal total)
        {
            InitializeComponent();
            _total = total;
            lblTotal.Text = _total.ToString("C2");

            // Valor inicial
            MetodoPagoSeleccionado = "Efectivo";
            txtPago.Focus();
        }

        // --- LÓGICA INTELIGENTE DE PAGO ---
        private void CmbMetodoPago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // --- LÍNEA DE SEGURIDAD ---
            // Si la ventana se está cargando y txtPago aún no existe, salimos.
            if (txtPago == null) return;
            // --------------------------

            if (cmbMetodoPago.SelectedItem is ComboBoxItem item)
            {
                MetodoPagoSeleccionado = item.Content.ToString();

                if (MetodoPagoSeleccionado == "Efectivo")
                {
                    txtPago.IsEnabled = true;
                    txtPago.Text = "";
                    txtPago.Focus();
                }
                else
                {
                    txtPago.Text = _total.ToString("0.00");
                    txtPago.IsEnabled = false;
                }
            }
        }

        private void TxtPago_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtPago.Text, out decimal pagaCon))
            {
                decimal vuelto = pagaCon - _total;
                lblVuelto.Text = vuelto.ToString("C2");
                lblVuelto.Foreground = vuelto >= 0 ? Brushes.Green : Brushes.Red;
            }
            else
            {
                lblVuelto.Text = "$ 0.00";
            }
        }

        private void TxtPago_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo números y puntos/comas
            Regex regex = new Regex("[^0-9,.]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtPago.Text, out decimal pagaCon))
            {
                // Validación: No puede pagar menos del total (especialmente en efectivo)
                // Nota: Usamos una pequeña tolerancia de 0.01 por temas de redondeo
                if (pagaCon < _total - 0.01m)
                {
                    MessageBox.Show("El pago es insuficiente.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                PagoRealizado = pagaCon;
                this.DialogResult = true; // Cierra con OK
            }
            else
            {
                MessageBox.Show("Ingrese un monto válido.");
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}