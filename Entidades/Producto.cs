using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entidades
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

      
        [Required]
        [MaxLength(50)]
        public string CodigoBarras { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; } 

        public bool TieneIVA { get; set; } = true; 
        
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 5; 
        public bool ControlarStock { get; set; } = true; 

       
        public bool Activo { get; set; } = true;

        
        public int? CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        public int? UnidadMedidaId { get; set; }
        [ForeignKey("UnidadMedidaId")]
        public virtual UnidadMedida UnidadMedida { get; set; }
    }
}