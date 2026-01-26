using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string? MetodoPago { get; set; } // "Efectivo", "Tarjeta", "QR"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; }

        public int UsuarioId { get; set; }

       
        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}   