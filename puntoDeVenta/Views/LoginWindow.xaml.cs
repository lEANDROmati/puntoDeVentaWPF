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

            
            txtUsuario.Focus();
        }

      
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

       
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

       
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsuario.Text;
            string pass = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Por favor, completa todos los campos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

           
            var usuarioEncontrado = await _usuarioService.LoginAsync(user, pass);

            if (usuarioEncontrado != null)
            {
                
                SesionActual.Usuario = usuarioEncontrado;

              
                MainWindow main = new MainWindow();
                main.Show();

                
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
