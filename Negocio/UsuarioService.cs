using Datos;
using Entidades;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;

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
        public Usuario Login(string nombre, string passwordPlana)
        {
            // 1. Buscamos al usuario SOLO por nombre primero
            var usuario = _context.Usuarios
                                  .FirstOrDefault(u => u.NombreUsuario == nombre && u.Activo);

            // 2. Si existe, verificamos la contraseña
            if (usuario != null)
            {
                // Verify toma la password escrita (ej: "123") y el hash de la BD ($2a$11$...)
                bool esValida = BCrypt.Net.BCrypt.Verify(passwordPlana, usuario.Password);

                if (esValida)
                {
                    return usuario; // Contraseña correcta
                }
            }

            return null; // Usuario no existe o contraseña incorrecta
        }
        public void RegistrarUsuario(Usuario usuario, string passwordPlana)
        {
            // Hasheamos la contraseña antes de guardar el objeto
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordPlana);

            // Guardamos
            Guardar(usuario); // Reutilizamos tu método Guardar existente
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
