using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades
{
    public class Configuracion
    {
        [Key]
        public int Id { get; set; }

        public string NombreNegocio { get; set; } = "Mi Negocio";
        public bool ImprimirTicket { get; set; } = true;
        public bool UsarControlCaja { get; set; } = true;
        public string? NombreImpresora { get; set; }


        public bool ManejarIVA { get; set; } = false; 

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeIVA { get; set; } = 21.00m; // Por defecto 21%

        public string Cuit { get; set; } // Identificación fiscal
        public string Direccion { get; set; }
    }
}