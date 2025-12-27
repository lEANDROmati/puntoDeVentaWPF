using Negocio;
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
    
    public partial class LoginWindow : Window
    {
        private readonly UsuarioService _service;

        public LoginWindow()
        {
            InitializeComponent();
            _service = new UsuarioService();
            txtUser.Focus();
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            var usuario = _service.Login(txtUser.Text, txtPass.Password);

            if (usuario != null)
            {
                // 1. Guardar en sesión
                SesionActual.Usuario = usuario;

                // 2. Abrir el sistema
                new MainWindow().Show();

                // 3. Cerrar el login
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPass.Clear();
            }
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
