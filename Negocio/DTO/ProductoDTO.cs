using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.DTO
{
    public class ProductoDto
    {
        // Datos directos (copia del producto)
        public int Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; } // Ya no el objeto, solo el nombre
        public string Unidad { get; set; }    // Solo la abreviatura

        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }

        // --- CAMPOS CALCULADOS (Aquí guardaremos el resultado) ---
        public decimal MargenGanancia { get; set; }
        public string EstadoStock { get; set; }
    }
}

