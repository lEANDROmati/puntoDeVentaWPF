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

        // --- IDENTIFICACIÓN ---
        [Required]
        [MaxLength(50)]
        public string CodigoBarras { get; set; } // Para el escáner

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        // --- PRECIOS Y COSTOS ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; } // Costo

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; } // Al público

        public bool TieneIVA { get; set; } = true; // Si este producto paga impuesto

        // --- STOCK ---
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 5; // Para alertas
        public bool ControlarStock { get; set; } = true; // Servicios (ej: Flete) no usan stock

        // --- ESTADO ---
        public bool Activo { get; set; } = true; // Soft Delete

        // --- RELACIONES (FOREIGN KEYS) ---
        public int? CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        public int? UnidadMedidaId { get; set; }
        [ForeignKey("UnidadMedidaId")]
        public virtual UnidadMedida UnidadMedida { get; set; }
    }
}