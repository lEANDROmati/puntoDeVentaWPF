using Datos;
using Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class CategoriaService
    {
        private readonly AppDbContext _context;

        public CategoriaService()
        {
            _context = new AppDbContext();
        }

        // 1. Obtener solo las activas
        public async Task<List<Categoria>> GetActivasAsync()
        {
            return await _context.Categorias
                           .Where(c => c.Activo)
                           .OrderBy(c => c.Nombre)
                           .ToListAsync();
        }

        // 2. Guardar o Editar (Upsert)
        public async Task GuardarAsync(Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new Exception("El nombre de la categoría es obligatorio.");

            if (categoria.Id == 0)
            {
                // Es Nueva
                await _context.Categorias.AddAsync(categoria);
            }
            else
            {
                // Es Edición
                _context.Categorias.Update(categoria);
            }
            await _context.SaveChangesAsync();
        }

        // 3. Eliminar (Borrado Lógico)
        public async Task EliminarAsync(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
              
                categoria.Activo = false;
                _context.Categorias.Update(categoria);
                await _context.SaveChangesAsync();
            }
        }
    }
}
