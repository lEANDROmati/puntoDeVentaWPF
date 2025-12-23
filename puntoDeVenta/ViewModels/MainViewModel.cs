using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Threading;

namespace puntoDeVenta.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // ViewModels disponibles
        public VentasViewModel VmCaja { get; } = new VentasViewModel();
        public InventarioViewModel VmInventario { get; } = new InventarioViewModel();
        public DashboardViewModel VmDashboard { get; } = new DashboardViewModel();
        public ConfiguracionViewModel VmConfiguracion { get; } = new ConfiguracionViewModel();

        // Vista Actual (Lo que se muestra en pantalla)
        [ObservableProperty]
        private object currentView;

        // Propiedad para la Hora
        [ObservableProperty]
        private string fechaHoraActual;

        public MainViewModel()
        {
            // Al iniciar, mostramos la Caja
            CurrentView = VmCaja;
            IniciarReloj();
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