
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    public class DetalleVenta
    {
        [Key]
        public int Id { get; set; }

       
        public int VentaId { get; set; }
        [ForeignKey("VentaId")]
        public Venta Venta { get; set; } = null!;

        
        public int ProductoId { get; set; }
        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; } = null!;

        public int Cantidad { get; set; }

       
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}