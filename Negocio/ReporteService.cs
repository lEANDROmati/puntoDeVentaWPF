using Datos;
using Negocio.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio
{
    public class ReporteService
    {
        private readonly AppDbContext _context;

        public ReporteService()
        {
            _context = new AppDbContext();
        }

        // 1. Ventas por Categoría (¿Qué rubro me da más plata?)
        public List<ReporteDto> ObtenerVentasPorCategoria(DateTime desde, DateTime hasta)
        {
            // Ajustamos fechas para incluir todo el último día
            DateTime fin = hasta.Date.AddDays(1).AddTicks(-1);
            DateTime inicio = desde.Date;

            var query = _context.DetallesVenta
                .Include(d => d.Producto)
                .ThenInclude(p => p.Categoria)
                .Where(d => d.Venta.Fecha >= inicio && d.Venta.Fecha <= fin)
                .GroupBy(d => d.Producto.Categoria.Nombre) // Agrupamos por nombre de categoría
                .Select(g => new ReporteDto
                {
                    Etiqueta = g.Key,
                    TotalVendido = g.Sum(x => x.Subtotal),
                    CantidadVendida = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.TotalVendido) // Ordenamos de mayor a menor $$
                .ToList();

            return query;
        }

        // 2. Top Productos (Los 5 más vendidos)
        public List<ReporteDto> ObtenerTopProductos(DateTime desde, DateTime hasta)
        {
            DateTime fin = hasta.Date.AddDays(1).AddTicks(-1);
            DateTime inicio = desde.Date;

            var query = _context.DetallesVenta
                .Include(d => d.Producto)
                .Where(d => d.Venta.Fecha >= inicio && d.Venta.Fecha <= fin)
                .GroupBy(d => d.Producto.Nombre) // Agrupamos por nombre del producto
                .Select(g => new ReporteDto
                {
                    Etiqueta = g.Key,
                    TotalVendido = g.Sum(x => x.Subtotal),
                    CantidadVendida = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.CantidadVendida) // Ordenamos por CANTIDAD
                .Take(5) // Solo los 5 mejores
                .ToList();

            return query;
        }
    }
}
