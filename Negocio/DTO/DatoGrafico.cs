using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.DTO
{
    public class DatoGrafico
    {
        public string Etiqueta { get; set; }  
        public decimal ValorReal { get; set; } 

        // Propiedades Visuales
        public double Altura { get; set; }     
        public string Color { get; set; }     
        public bool EsHoy { get; set; }       

       
        public string TooltipInfo => $"{Etiqueta}: {ValorReal:C2}";
    }
}