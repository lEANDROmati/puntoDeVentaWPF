using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.DTO
{
    public class ReporteDto
    {
        public string Etiqueta { get; set; } // Ej: "Coca Cola" o "Bebidas"
        public decimal TotalVendido { get; set; } // Dinero recaudado
        public int CantidadVendida { get; set; } // Unidades vendidas
    }
}
