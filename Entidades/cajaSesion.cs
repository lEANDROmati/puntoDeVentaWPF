using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    public class CajaSesion
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaApertura { get; set; } = DateTime.Now;
        public DateTime? FechaCierre { get; set; } // Null significa que está abierta

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoInicial { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontoFinal { get; set; }

        public bool EstaAbierta { get; set; } = true;
    }
}