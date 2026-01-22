using Datos;
using Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Negocio
{
    public class CajaService
    {
        // Verificar si hay una caja abierta
        public async Task<CajaSesion?> ObtenerCajaAbiertaAsync()
        {
            using (var db = new AppDbContext())
            {
                return await db.CajasSesiones
                         .OrderByDescending(c => c.Id)
                         .FirstOrDefaultAsync(c => c.EstaAbierta);
            }
        }

        public async Task AbrirCajaAsync(decimal montoInicial)
        {
            using (var db = new AppDbContext())
            {
                if (await ObtenerCajaAbiertaAsync() != null)
                    throw new Exception("Ya hay una caja abierta.");

                var nuevaSesion = new CajaSesion 
                {
                    MontoInicial = montoInicial,
                    FechaApertura = DateTime.Now,
                    EstaAbierta = true
                };
                await db.CajasSesiones.AddAsync(nuevaSesion);
                await db.SaveChangesAsync();
            }
        }

        public async Task CerrarCajaAsync(decimal montoFinal)
        {
            using (var db = new AppDbContext())
            {
                var caja = await db.CajasSesiones.FirstOrDefaultAsync(c => c.EstaAbierta);
                if (caja == null) throw new Exception("No hay caja abierta para cerrar.");

                caja.FechaCierre = DateTime.Now;
                caja.MontoFinal = montoFinal;
                caja.EstaAbierta = false;

                await db.SaveChangesAsync();
            }
        }
    }
}