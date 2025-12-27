using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Negocio;       // Tu capa de negocio
using Entidades;     // Tus entidades
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

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

            CargarDatos();
        }

        public void CargarDatos()
        {
            Categorias = new ObservableCollection<Categoria>(_categoriaService.GetActivas());
            Unidades = new ObservableCollection<UnidadMedida>(_unidadService.GetActivas());
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
        private void GuardarCategoria()
        {
            if (string.IsNullOrWhiteSpace(NombreCategoria)) return;

            var cat = CategoriaSeleccionada ?? new Categoria();
            cat.Nombre = NombreCategoria;
            cat.Activo = true; // Por defecto

            _categoriaService.Guardar(cat); // Asegúrate que tu servicio tenga este método

            NuevaCategoria(); // Limpiar
            CargarDatos();    // Refrescar lista
        }

        [RelayCommand]
        private void EliminarCategoria()
        {
            if (CategoriaSeleccionada == null) return;

            // Aquí idealmente validas si hay productos usando esta categoría antes de borrar
            _categoriaService.Eliminar(CategoriaSeleccionada.Id); // O desactivar

            NuevaCategoria();
            CargarDatos();
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
        private void GuardarUnidad()
        {
            if (string.IsNullOrWhiteSpace(NombreUnidad)) return;

            var uni = UnidadSeleccionada ?? new UnidadMedida();
            uni.Nombre = NombreUnidad;
            uni.Abreviatura = AbrevUnidad;
            uni.Activo = true;

            _unidadService.Guardar(uni);

            NuevaUnidad();
            CargarDatos();
        }

        [RelayCommand]
        private void EliminarUnidad()
        {
            if (UnidadSeleccionada == null) return;

            _unidadService.Eliminar(UnidadSeleccionada.Id);

            NuevaUnidad();
            CargarDatos();
        }
    }
}