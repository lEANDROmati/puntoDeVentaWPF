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
        private readonly UsuarioService _usuarioService;

        public LoginWindow()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();

            // Foco inicial en usuario
            txtUsuario.Focus();
        }

        // 1. PERMITIR ARRASTRAR LA VENTANA (Porque quitamos los bordes)
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // 2. BOTÓN CERRAR (X)
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // 3. LÓGICA DE LOGIN
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text;
            string pass = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Por favor, completa todos los campos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Llamamos al servicio
            var usuarioEncontrado = await _usuarioService.LoginAsync(user, pass);

            if (usuarioEncontrado != null)
            {
                // Guardamos la sesión
                SesionActual.Usuario = usuarioEncontrado;

                // Abrimos la ventana principal
                MainWindow main = new MainWindow();
                main.Show();

                // Cerramos el login
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error de Acceso", MessageBoxButton.OK, MessageBoxImage.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
    }
}
