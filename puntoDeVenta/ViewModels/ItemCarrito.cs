using CommunityToolkit.Mvvm.ComponentModel;
using Entidades;


namespace puntoDeVenta.ViewModels
{
   
    public partial class ItemCarrito : ObservableObject
    {
        public Producto Producto { get; set; }
        public decimal PrecioUnitario { get; set; }

        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Subtotal))]
        private int cantidad;

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
