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
    public class UnidadMedidaService
    {
        private readonly AppDbContext _context;

        public UnidadMedidaService()
        {
            _context = new AppDbContext();
        }

        public async Task<List<UnidadMedida>> GetActivasAsync()
        {
            return await _context.UnidadesMedida
                           .Where(u => u.Activo)
                           .OrderBy(u => u.Nombre)
                           .ToListAsync();
        }

        public async Task GuardarAsync(UnidadMedida unidad)
        {
            if (string.IsNullOrWhiteSpace(unidad.Nombre))
                throw new Exception("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(unidad.Abreviatura))
                throw new Exception("La abreviatura es obligatoria (ej: kg, un).");

            if (unidad.Id == 0)
            {
                await _context.UnidadesMedida.AddAsync(unidad);
            }
            else
            {
                _context.UnidadesMedida.Update(unidad);
            }
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var unidad = await _context.UnidadesMedida.FindAsync(id);
            if (unidad != null)
            {
                unidad.Activo = false;
                _context.UnidadesMedida.Update(unidad);
                await _context.SaveChangesAsync();
            }
        }
    }
}
