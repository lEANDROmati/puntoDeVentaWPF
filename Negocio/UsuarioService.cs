using Datos;
using Entidades;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<Usuario> LoginAsync(string nombre, string passwordPlana)
        {
            // 1. Buscamos al usuario SOLO por nombre primero
            var usuario = await _context.Usuarios
                                  .FirstOrDefaultAsync(u => u.NombreUsuario == nombre && u.Activo);

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
        public async Task RegistrarUsuarioAsync(Usuario usuario, string passwordPlana)
        {
            // Hasheamos la contraseña antes de guardar el objeto
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordPlana);

            // Guardamos
            await GuardarAsync(usuario); // Reutilizamos tu método Guardar existente
        }

        // 2. LISTAR TODOS (Para la tabla)
        public async Task<List<Usuario>> ObtenerTodosAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // 3. GUARDAR (Nuevo o Edición)
        public async Task GuardarAsync(Usuario usuario)
        {
            if (usuario.Id == 0)
            {
                await _context.Usuarios.AddAsync(usuario); // Nuevo
            }
            else
            {
                _context.Usuarios.Update(usuario); // Editar
            }
            await _context.SaveChangesAsync();
        }

        // 4. ELIMINAR
        public async Task EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
        }

        // 5. VALIDAR SI EXISTE (Para no repetir nombres)
        public async Task<bool> ExisteUsuarioAsync(string nombre)
        {
            return await _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombre);
        }
    }
}
