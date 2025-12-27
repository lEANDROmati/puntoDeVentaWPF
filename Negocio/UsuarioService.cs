using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService()
        {
            _context = new AppDbContext();
        }

        public Usuario Login(string usuario, string password)
        {
            // Busca un usuario activo que coincida
            return _context.Usuarios
                           .FirstOrDefault(u => u.Activo &&
                                                u.NombreUsuario == usuario &&
                                                u.Password == password);
        }
    }
}
