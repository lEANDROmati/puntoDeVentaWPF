using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    public class UnidadMedida
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } // Ej: Kilogramo, Litro, Unidad

        [MaxLength(10)]
        public string Abreviatura { get; set; } // Ej: kg, lt, un

        public bool Activo { get; set; } = true;
    }
}