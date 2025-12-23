using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio
{
    public class CategoriaService
    {
        private readonly AppDbContext _context;

        public CategoriaService()
        {
            _context = new AppDbContext();
        }

        // 1. Obtener solo las activas (para llenar ComboBoxes)
        public List<Categoria> GetActivas()
        {
            return _context.Categorias
                           .Where(c => c.Activo)
                           .OrderBy(c => c.Nombre)
                           .ToList();
        }

        // 2. Guardar o Editar (Upsert)
        public void Guardar(Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new Exception("El nombre de la categoría es obligatorio.");

            if (categoria.Id == 0)
            {
                // Es Nueva
                _context.Categorias.Add(categoria);
            }
            else
            {
                // Es Edición
                _context.Categorias.Update(categoria);
            }
            _context.SaveChanges();
        }

        // 3. Eliminar (Borrado Lógico)
        public void Eliminar(int id)
        {
            var categoria = _context.Categorias.Find(id);
            if (categoria != null)
            {
                // NO hacemos Remove. Solo la desactivamos.
                // Así los productos viejos no pierden su categoría histórica.
                categoria.Activo = false;
                _context.Categorias.Update(categoria);
                _context.SaveChanges();
            }
        }
    }
}
