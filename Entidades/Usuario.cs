using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entidades
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreUsuario { get; set; } // El que usa para loguearse (ej: "admin")

        [Required]
        public string Password { get; set; }      // La contraseña

        [MaxLength(100)]
        public string NombreCompleto { get; set; } // Nombre real (ej: "Juan Perez")

        public string Rol { get; set; } = "Cajero"; // "Admin" o "Cajero"
        public bool Activo { get; set; } = true;
    }
}
