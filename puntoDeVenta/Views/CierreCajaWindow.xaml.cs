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
using System.Windows.Shapes;


namespace puntoDeVenta.Views
{
    public partial class CierreCajaWindow : Window
    {
        public decimal MontoRealEnCaja { get; private set; }
        private decimal _montoEsperado;

        // Modificamos el constructor para recibir el esperado
        public CierreCajaWindow(decimal montoEsperado)
        {
            InitializeComponent();
            _montoEsperado = montoEsperado;

            // Mostramos el monto esperado en el texto que agregaremos en el paso 2
            lblEsperado.Text = $"Sistema espera: ${_montoEsperado:N2}";

            // Foco rápido
            Loaded += (s, e) => { txtMonto.Focus(); txtMonto.SelectAll(); };

            // Evento para calcular diferencia en tiempo real mientras escribes
            txtMonto.TextChanged += TxtMonto_TextChanged;
        }

        private void TxtMonto_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal real))
            {
                decimal diferencia = real - _montoEsperado;
                lblDiferencia.Text = $"Diferencia: ${diferencia:N2}";

                // Verde si sobra o está exacto, Rojo si falta
                lblDiferencia.Foreground = diferencia >= 0 ? Brushes.Green : Brushes.Red;
            }
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal monto))
            {
                MontoRealEnCaja = monto;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Ingresa un número válido.");
            }
        }
    }
}
