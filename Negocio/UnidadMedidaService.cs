using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio
{
    public class UnidadMedidaService
    {
        private readonly AppDbContext _context;

        public UnidadMedidaService()
        {
            _context = new AppDbContext();
        }

        public List<UnidadMedida> GetActivas()
        {
            return _context.UnidadesMedida
                           .Where(u => u.Activo)
                           .OrderBy(u => u.Nombre)
                           .ToList();
        }

        public void Guardar(UnidadMedida unidad)
        {
            if (string.IsNullOrWhiteSpace(unidad.Nombre))
                throw new Exception("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(unidad.Abreviatura))
                throw new Exception("La abreviatura es obligatoria (ej: kg, un).");

            if (unidad.Id == 0)
            {
                _context.UnidadesMedida.Add(unidad);
            }
            else
            {
                _context.UnidadesMedida.Update(unidad);
            }
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var unidad = _context.UnidadesMedida.Find(id);
            if (unidad != null)
            {
                unidad.Activo = false;
                _context.UnidadesMedida.Update(unidad);
                _context.SaveChanges();
            }
        }
    }
}
