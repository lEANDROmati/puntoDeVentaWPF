using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entidades;
using Negocio;
using System.Collections.ObjectModel;
using System.Printing;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace puntoDeVenta.ViewModels
{
    public partial class ConfiguracionViewModel : ObservableObject
    {
        private readonly ConfigService _configService;
        private readonly UsuarioService _usuarioService;

        // Esta variable guarda la configuración real de la BD
        private Configuracion _configActual;

        public ConfiguracionViewModel()
        {
            _configService = new ConfigService();
            _usuarioService = new UsuarioService();

            ListaUsuarios = new ObservableCollection<Usuario>();

            _ = CargarDatos();
            _ = CargarUsuarios();
        }

        // --- PROPIEDADES ENLAZADAS A LA PANTALLA ---
        [ObservableProperty] private string nombreNegocio;
        [ObservableProperty] private string direccionNegocio; 
        [ObservableProperty] private bool imprimirTicket;
        [ObservableProperty] private bool usarControlCaja;
        [ObservableProperty] private string nombreImpresora;


        public List<string> ListaImpresoras { get; set; } = new List<string>();


        [ObservableProperty] private string nuevoUsuarioNombre;
        [ObservableProperty] private string nuevoUsuarioPass;
        [ObservableProperty] private string nuevoUsuarioRol; 

        public ObservableCollection<Usuario> ListaUsuarios { get; set; }
        public List<string> ListaRoles { get; } = new List<string> { "Admin", "cajero" };
        
        private async Task CargarDatos()
        {
           
            _configActual = await _configService.ObtenerConfigAsync();

           
            NombreNegocio = _configActual.NombreNegocio;
            ImprimirTicket = _configActual.ImprimirTicket;
            UsarControlCaja = _configActual.UsarControlCaja;
            NombreImpresora = _configActual.NombreImpresora;
           

            CargarImpresorasSistema();
        }

        private async Task CargarUsuarios()
        {
            ListaUsuarios.Clear();
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            foreach (var u in usuarios) ListaUsuarios.Add(u);
        }

        [RelayCommand]
        private async Task CrearUsuario()
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(NuevoUsuarioNombre) || string.IsNullOrWhiteSpace(NuevoUsuarioPass))
            {
                MessageBox.Show("Debes escribir un nombre y contraseña.");
                return;
            }
            if (string.IsNullOrEmpty(NuevoUsuarioRol))
            {
                MessageBox.Show("Debes seleccionar un Rol (Admin o Empleado).");
                return;
            }
            if (await _usuarioService.ExisteUsuarioAsync(NuevoUsuarioNombre))
            {
                MessageBox.Show("Ya existe un usuario con ese nombre.");
                return;
            }

            // Crear
            var nuevo = new Usuario
            {
                NombreUsuario = NuevoUsuarioNombre,              
                Rol = NuevoUsuarioRol,
                NombreCompleto = NuevoUsuarioNombre, 
                Activo = true
            };

            await _usuarioService.RegistrarUsuarioAsync(nuevo, NuevoUsuarioPass);

            // Limpiar y Recargar
            NuevoUsuarioNombre = "";
            NuevoUsuarioPass = "";
            await CargarUsuarios();

            MessageBox.Show($"Usuario '{nuevo.NombreUsuario}' creado con éxito y protegido.");
        }

        [RelayCommand]
        private async Task EliminarUsuario(Usuario usuario)
        {
            if (usuario == null) return;

            // Protección: No borrar al último Admin
            if (usuario.Rol == "Admin" && ListaUsuarios.Count(u => u.Rol == "Admin") <= 1)
            {
                MessageBox.Show("No puedes eliminar al último Administrador.");
                return;
            }

            if (MessageBox.Show($"¿Eliminar a {usuario.NombreUsuario}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _usuarioService.EliminarAsync(usuario.Id);
                await CargarUsuarios();
            }
        }

        private void CargarImpresorasSistema()
        {
            ListaImpresoras.Clear();
            try
            {
                var server = new LocalPrintServer();
                foreach (var cola in server.GetPrintQueues())
                {
                    ListaImpresoras.Add(cola.Name); 
                }
                OnPropertyChanged(nameof(ListaImpresoras));
            }
            catch { }
        }

        [RelayCommand]
        private async Task GuardarCambios()
        {
            try
            {
                
                _configActual.NombreNegocio = NombreNegocio;
                _configActual.ImprimirTicket = ImprimirTicket;
                _configActual.UsarControlCaja = UsarControlCaja;
                _configActual.NombreImpresora = NombreImpresora;

                
                await _configService.GuardarConfigAsync(_configActual);

                MessageBox.Show("¡Configuración guardada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}");
            }
        }
    }
}
