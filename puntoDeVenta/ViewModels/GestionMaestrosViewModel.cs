using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Negocio;       // Tu capa de negocio
using Entidades;     // Tus entidades
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Threading.Tasks;

namespace puntoDeVenta.ViewModels
{
    public partial class GestionMaestrosViewModel : ObservableObject
    {
        private readonly CategoriaService _categoriaService;
        private readonly UnidadMedidaService _unidadService;

        // --- COLECCIONES ---
        [ObservableProperty] private ObservableCollection<Categoria> categorias;
        [ObservableProperty] private ObservableCollection<UnidadMedida> unidades;

        // --- SELECCIONADOS (Para editar) ---
        private Categoria categoriaSeleccionada;
        public Categoria CategoriaSeleccionada
        {
            get => categoriaSeleccionada;
            set
            {
                if (SetProperty(ref categoriaSeleccionada, value))
                {
                    // Al seleccionar de la lista, llenamos los campos de texto
                    if (value != null)
                    {
                        NombreCategoria = value.Nombre;
                    }
                }
            }
        }

        private UnidadMedida unidadSeleccionada;
        public UnidadMedida UnidadSeleccionada
        {
            get => unidadSeleccionada;
            set
            {
                if (SetProperty(ref unidadSeleccionada, value))
                {
                    if (value != null)
                    {
                        NombreUnidad = value.Nombre;
                        AbrevUnidad = value.Abreviatura;
                    }
                }
            }
        }

        // --- CAMPOS DE TEXTO (Formulario) ---
        [ObservableProperty] private string nombreCategoria;

        [ObservableProperty] private string nombreUnidad;
        [ObservableProperty] private string abrevUnidad;

        public GestionMaestrosViewModel()
        {
            _categoriaService = new CategoriaService();
            _unidadService = new UnidadMedidaService();

            _ = CargarDatos();
        }

        public async Task CargarDatos()
        {
            var cats = await _categoriaService.GetActivasAsync();
            var units = await _unidadService.GetActivasAsync();
            Categorias = new ObservableCollection<Categoria>(cats);
            Unidades = new ObservableCollection<UnidadMedida>(units);
        }

        // ==========================================
        // COMANDOS CATEGORÍAS
        // ==========================================

        [RelayCommand]
        private void NuevaCategoria()
        {
            CategoriaSeleccionada = null; // Deseleccionar
            NombreCategoria = "";         // Limpiar
        }

        [RelayCommand]
        private async Task GuardarCategoria()
        {
            if (string.IsNullOrWhiteSpace(NombreCategoria)) return;

            var cat = CategoriaSeleccionada ?? new Categoria();
            cat.Nombre = NombreCategoria;
            cat.Activo = true; // Por defecto

            await _categoriaService.GuardarAsync(cat); // Asegúrate que tu servicio tenga este método

            NuevaCategoria(); // Limpiar
            await CargarDatos();    // Refrescar lista
        }

        [RelayCommand]
        private async Task EliminarCategoria()
        {
            if (CategoriaSeleccionada == null) return;

            // Aquí idealmente validas si hay productos usando esta categoría antes de borrar
            await _categoriaService.EliminarAsync(CategoriaSeleccionada.Id); // O desactivar

            NuevaCategoria();
            await CargarDatos();
        }

        // ==========================================
        // COMANDOS UNIDADES
        // ==========================================

        [RelayCommand]
        private void NuevaUnidad()
        {
            UnidadSeleccionada = null;
            NombreUnidad = "";
            AbrevUnidad = "";
        }

        [RelayCommand]
        private async Task GuardarUnidad()
        {
            if (string.IsNullOrWhiteSpace(NombreUnidad)) return;

            var uni = UnidadSeleccionada ?? new UnidadMedida();
            uni.Nombre = NombreUnidad;
            uni.Abreviatura = AbrevUnidad;
            uni.Activo = true;

            await _unidadService.GuardarAsync(uni);

            NuevaUnidad();
            await CargarDatos();
        }

        [RelayCommand]
        private async Task EliminarUnidad()
        {
            if (UnidadSeleccionada == null) return;

            await _unidadService.EliminarAsync(UnidadSeleccionada.Id);

            NuevaUnidad();
            await CargarDatos();
        }
    }
}
