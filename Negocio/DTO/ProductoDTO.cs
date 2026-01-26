using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.DTO
{
    public class ProductoDto
    {
       
        public int Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; } 
        public string Unidad { get; set; }   

        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }

       
        public decimal MargenGanancia { get; set; }
        public string EstadoStock { get; set; }


    }
}

