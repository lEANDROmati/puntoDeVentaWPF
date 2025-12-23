
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

        // RELACIÓN CON VENTA (Foreign Key)
        public int VentaId { get; set; }
        [ForeignKey("VentaId")]
        public Venta Venta { get; set; } = null!;

        // RELACIÓN CON PRODUCTO (Foreign Key)
        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; } = null!;

        public int Cantidad { get; set; }

        // IMPORTANTE: Guardamos el precio aquí para historial.
        // Si el producto cambia de precio mañana, esta venta vieja NO cambia.
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}