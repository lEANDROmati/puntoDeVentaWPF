using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    public class UnidadMedida
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } 

        [MaxLength(10)]
        public string Abreviatura { get; set; } 

        public bool Activo { get; set; } = true;
    }
}