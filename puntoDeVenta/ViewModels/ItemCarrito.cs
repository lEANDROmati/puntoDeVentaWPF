using CommunityToolkit.Mvvm.ComponentModel;
using Entidades;


namespace puntoDeVenta.ViewModels
{
    // Esta clase SOLO sirve para la pantalla de Ventas.
    // Aquí sí podemos usar ObservableObject sin ensuciar la Entidad real.
    public partial class ItemCarrito : ObservableObject
    {
        public Producto Producto { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Aquí está la magia: Al cambiar Cantidad, avisa al Subtotal
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Subtotal))]
        private int cantidad;

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
