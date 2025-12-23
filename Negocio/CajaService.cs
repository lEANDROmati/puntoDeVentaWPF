using Datos;
using Entidades;

using System;
using System.Linq;

namespace Negocio
{
    public class CajaService
    {
        // Verificar si hay una caja abierta
        public CajaSesion? ObtenerCajaAbierta()
        {
            using (var db = new AppDbContext())
            {
                return db.CajasSesiones
                         .OrderByDescending(c => c.Id)
                         .FirstOrDefault(c => c.EstaAbierta);
            }
        }

        public void AbrirCaja(decimal montoInicial)
        {
            using (var db = new AppDbContext())
            {
                if (ObtenerCajaAbierta() != null)
                    throw new Exception("Ya hay una caja abierta.");

                var nuevaSesion = new CajaSesion { MontoInicial = montoInicial };
                db.CajasSesiones.Add(nuevaSesion);
                db.SaveChanges();
            }
        }

        public void CerrarCaja(decimal montoFinal)
        {
            using (var db = new AppDbContext())
            {
                var caja = db.CajasSesiones.FirstOrDefault(c => c.EstaAbierta);
                if (caja == null) throw new Exception("No hay caja abierta para cerrar.");

                caja.FechaCierre = DateTime.Now;
                caja.MontoFinal = montoFinal;
                caja.EstaAbierta = false;

                db.SaveChanges();
            }
        }
    }
}