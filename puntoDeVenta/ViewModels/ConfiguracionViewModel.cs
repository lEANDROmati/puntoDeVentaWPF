using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entidades;
using Negocio;
using System.Windows;

namespace puntoDeVenta.ViewModels
{
    public partial class ConfiguracionViewModel : ObservableObject
    {
        private readonly ConfigService _configService;

        // Esta variable guarda la configuración real de la BD
        private Configuracion _configActual;

        public ConfiguracionViewModel()
        {
            _configService = new ConfigService();
            CargarDatos();
        }

        // --- PROPIEDADES ENLAZADAS A LA PANTALLA ---
        [ObservableProperty] private string nombreNegocio;
        [ObservableProperty] private string direccionNegocio; // Campo nuevo sugerido
        [ObservableProperty] private bool imprimirTicket;
        [ObservableProperty] private bool usarControlCaja;

        private void CargarDatos()
        {
            // 1. Buscamos la configuración en la BD
            _configActual = _configService.ObtenerConfig();

            // 2. Pasamos los datos a la pantalla
            NombreNegocio = _configActual.NombreNegocio;
            ImprimirTicket = _configActual.ImprimirTicket;
            UsarControlCaja = _configActual.UsarControlCaja;
            // DireccionNegocio = ... (si agregas el campo a la entidad después)
        }

        [RelayCommand]
        private void GuardarCambios()
        {
            try
            {
                // 1. Actualizamos el objeto original
                _configActual.NombreNegocio = NombreNegocio;
                _configActual.ImprimirTicket = ImprimirTicket;
                _configActual.UsarControlCaja = UsarControlCaja;

                // 2. Guardamos en BD
                _configService.GuardarConfig(_configActual);

                MessageBox.Show("¡Configuración guardada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}");
            }
        }
    }
}