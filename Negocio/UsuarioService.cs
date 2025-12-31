using Datos;
using Entidades;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService()
        {
            _context = new AppDbContext();
        }

        // 1. LOGIN (Ya lo tenías)
        public Usuario Login(string nombre, string password)
        {
            return _context.Usuarios
                           .FirstOrDefault(u => u.NombreUsuario == nombre && u.Password == password);
        }

        // 2. LISTAR TODOS (Para la tabla)
        public List<Usuario> ObtenerTodos()
        {
            return _context.Usuarios.ToList();
        }

        // 3. GUARDAR (Nuevo o Edición)
        public void Guardar(Usuario usuario)
        {
            if (usuario.Id == 0)
            {
                _context.Usuarios.Add(usuario); // Nuevo
            }
            else
            {
                _context.Usuarios.Update(usuario); // Editar
            }
            _context.SaveChanges();
        }

        // 4. ELIMINAR
        public void Eliminar(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }
        }

        // 5. VALIDAR SI EXISTE (Para no repetir nombres)
        public bool ExisteUsuario(string nombre)
        {
            return _context.Usuarios.Any(u => u.NombreUsuario == nombre);
        }
    }
}
