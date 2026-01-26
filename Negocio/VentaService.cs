using Datos;
using DocumentFormat.OpenXml.InkML;
using Entidades;
using Microsoft.EntityFrameworkCore; 
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
           
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                   
                    var nuevaVenta = new Venta
                    {
                        Fecha = DateTime.Now,
                        Total = total,
                        Importe = importe,      
                        MetodoPago = metodoPago, 
                        UsuarioId = SesionActual.Usuario.Id
                       
                    };

                    await _context.Ventas.AddAsync(nuevaVenta);
                    await _context.SaveChangesAsync(); 

                    //  Procesar Detalles y Stock
                    foreach (var item in detalles)
                    {
                        item.VentaId = nuevaVenta.Id;
                        item.Producto = null; 
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
                    await transaction.CommitAsync(); 
                    return nuevaVenta.Id;
                }

                catch (Exception)
                {
                    await transaction.RollbackAsync(); 
                    throw;
                }
            }
        }

        public async Task<List<Venta>> GetVentasPorFechaAsync(DateTime desde, DateTime hasta)
        {
            using (var _context = new AppDbContext())
            {
                
                DateTime fechaFin = hasta.Date.AddDays(1).AddTicks(-1);
                DateTime fechaInicio = desde.Date;

                return await _context.Ventas
                               .Include(v => v.Detalles)          
                               .ThenInclude(d => d.Producto)     
                               .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
                               .OrderByDescending(v => v.Fecha)   
                               .ToListAsync();
            }
        }
    }
}
