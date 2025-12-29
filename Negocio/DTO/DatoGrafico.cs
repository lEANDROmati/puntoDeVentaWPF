using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.DTO
{
    public class DatoGrafico
    {
        public string Etiqueta { get; set; }   // Ej: "10:00", "Lun", "05/12"
        public decimal ValorReal { get; set; } // Ej: 15000.00

        // Propiedades Visuales
        public double Altura { get; set; }     // Altura calculada en pixeles (ej: 100)
        public string Color { get; set; }      // Ej: "#3F51B5" (Azul) o "#E0E0E0" (Gris)
        public bool EsHoy { get; set; }        // Para resaltar la barra actual

        // Tooltip para ver el valor al pasar el mouse
        public string TooltipInfo => $"{Etiqueta}: {ValorReal:C2}";
    }
}