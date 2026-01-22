using Datos;
using DocumentFormat.OpenXml.InkML;
using Entidades;
using Microsoft.EntityFrameworkCore; // Necesario para .Include()
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Negocio
{
    public class VentaService
    {
        private readonly AppDbContext _context;

        public VentaService()
        {
            _context = new AppDbContext();
        }

        public async Task<int> GuardarVentaAsync(decimal total, List<DetalleVenta> detalles, decimal importe, string metodoPago)
        {
            // Transacción: O se guarda todo, o no se guarda nada (Seguridad)
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Crear la Venta (Cabecera)
                    var nuevaVenta = new Venta
                    {
                        Fecha = DateTime.Now,
                        Total = total,
                        Importe = importe,       // Guardamos cuánto pagó
                        MetodoPago = metodoPago, // Guardamos cómo pagó (Efectivo/Tarjeta/QR)
                        UsuarioId = SesionActual.Usuario.Id
                       //UsuarioId = 1 // Temporal hasta tener sesión
                    };

                    await _context.Ventas.AddAsync(nuevaVenta);
                    await _context.SaveChangesAsync(); // Guardamos para obtener el ID

                    // 2. Procesar Detalles y Stock
                    foreach (var item in detalles)
                    {
                        item.VentaId = nuevaVenta.Id;
                        item.Producto = null; // Limpiamos referencia para evitar duplicados
                        await _context.DetallesVenta.AddAsync(item);

                        // Descontar Stock
                        var productoEnBd = await _context.Productos.FindAsync(item.ProductoId);
                        if (productoEnBd != null)
                        {
                            productoEnBd.Stock -= item.Cantidad;

                            // Validación de Stock Negativo
                            if (productoEnBd.ControlarStock && productoEnBd.Stock < 0)
                            {
                                throw new Exception($"Stock insuficiente para '{productoEnBd.Nombre}'.");
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync(); // Confirmar cambios
                    return nuevaVenta.Id;
                }

                catch (Exception)
                {
                    await transaction.RollbackAsync(); // Deshacer si hubo error
                    throw;
                }
            }
        }

        public async Task<List<Venta>> GetVentasPorFechaAsync(DateTime desde, DateTime hasta)
        {
            using (var _context = new AppDbContext())
            {
                // Truco: Ajustamos la hora para abarcar todo el día final (23:59:59)
                DateTime fechaFin = hasta.Date.AddDays(1).AddTicks(-1);
                DateTime fechaInicio = desde.Date;

                return await _context.Ventas
                               .Include(v => v.Detalles)          // Traer los detalles
                               .ThenInclude(d => d.Producto)      // Traer nombre del producto en el detalle
                               .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
                               .OrderByDescending(v => v.Fecha)   // Las más nuevas arriba
                               .ToListAsync();
            }
        }
    }
}
