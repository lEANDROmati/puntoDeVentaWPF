using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Negocio;
using System;
using System.Windows;
using System.Windows.Threading;

namespace puntoDeVenta.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
       
        public VentasViewModel VmCaja { get; } = new VentasViewModel();
        public InventarioViewModel VmInventario { get; } = new InventarioViewModel();
        public HistorialViewModel VmHistorial { get; } = new HistorialViewModel();
        public DashboardViewModel VmDashboard { get; } = new DashboardViewModel();
        public ConfiguracionViewModel VmConfiguracion { get; } = new ConfiguracionViewModel();
        public ReportesViewModel VmReportes { get; } = new ReportesViewModel(); 

     

        [ObservableProperty]
        private Visibility menuVisibility = Visibility.Visible; // Menú lateral

        [ObservableProperty]
        private Visibility homeButtonVisibility = Visibility.Collapsed; // Botón Home

        partial void OnCurrentViewChanged(object value)
        {
           
            if (value is VentasViewModel)
            {
               
                MenuVisibility = Visibility.Collapsed;
                HomeButtonVisibility = Visibility.Visible;
            }
            else
            {
                
                MenuVisibility = Visibility.Visible;
                HomeButtonVisibility = Visibility.Collapsed;
            }
        }

        [ObservableProperty]
        private object currentView;

       
        [ObservableProperty]
        private string fechaHoraActual;

        public bool EsAdmin { get; set; }

        public MainViewModel()
        {
            if (SesionActual.Usuario != null && SesionActual.Usuario.Rol == "Admin")
            {
                EsAdmin = true;
            }
            else
            {
                EsAdmin = false;
            }
            
            CurrentView = VmDashboard;
            IniciarReloj();
            _ = CargarConfiguracion();
        }

        // --- COMANDOS DE NAVEGACIÓN ---



        [RelayCommand]
        private void IrACaja() => CurrentView = VmCaja;

        [RelayCommand]
        private void IrAInventario() => CurrentView = VmInventario;

        [RelayCommand]
        private void IrADashboard() => CurrentView = VmDashboard;

        [RelayCommand]
        private void IrAConfiguracion() => CurrentView = VmConfiguracion;

        [RelayCommand]
        private void IrAHistorial() => CurrentView = VmHistorial;

        [RelayCommand] 
        private void IrAReportes() => CurrentView = VmReportes;

        [RelayCommand]
        private void VolverAlHome() => CurrentView = VmDashboard;


        [ObservableProperty]
        private string nombreSucursal;
        public async System.Threading.Tasks.Task CargarConfiguracion()
        {
            try
            {
                var configService = new ConfigService();
                var config = await configService.ObtenerConfigAsync();
                NombreSucursal = config.NombreNegocio;
            }
            catch
            {
                NombreSucursal = "Punto de Venta";
            }
        }


        // --- RELOJ ---
        private void IniciarReloj()
        {
            FechaHoraActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => FechaHoraActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            timer.Start();
        }
    }
}