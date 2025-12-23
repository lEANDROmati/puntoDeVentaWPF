using Datos;
using DocumentFormat.OpenXml.InkML;
using Entidades;
using Microsoft.EntityFrameworkCore; // Necesario para .Include()
using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    public class VentaService
    {
        private readonly AppDbContext _context;

        public VentaService()
        {
            _context = new AppDbContext();
        }

        public void GuardarVenta(decimal total, List<DetalleVenta> detalles, decimal importe, string metodoPago)
        {
            // Transacción: O se guarda todo, o no se guarda nada (Seguridad)
            using (var transaction = _context.Database.BeginTransaction())
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
                        UsuarioId = 1            // Hardcodeado (luego pondremos el usuario real)
                    };

                    _context.Ventas.Add(nuevaVenta);
                    _context.SaveChanges(); // Guardamos para obtener el ID

                    // 2. Procesar Detalles y Stock
                    foreach (var item in detalles)
                    {
                        item.VentaId = nuevaVenta.Id;
                        item.Producto = null; // Limpiamos referencia para evitar duplicados
                        _context.DetallesVenta.Add(item);

                        // Descontar Stock
                        var productoEnBd = _context.Productos.Find(item.ProductoId);
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

                    _context.SaveChanges();
                    transaction.Commit(); // Confirmar cambios
                }
                catch (Exception)
                {
                    transaction.Rollback(); // Deshacer si hubo error
                    throw;
                }
            }
        }
    }
}