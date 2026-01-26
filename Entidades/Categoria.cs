using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } 

        public bool Activo { get; set; } = true; 
    }
}
