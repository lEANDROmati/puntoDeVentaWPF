using Entidades;

using System.IO;
using System.Text;

namespace Negocio
{
    public class TicketService
    {
        public void ImprimirTicket(Venta venta, string nombreNegocio)
        {
            // AQUÍ IRÍA LA LÓGICA DE IMPRESORA REAL (ESC/POS)
            // Por ahora, generaremos un archivo de texto en el escritorio como simulación

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("================================");
            sb.AppendLine($"      {nombreNegocio.ToUpper()}      ");
            sb.AppendLine("================================");
            sb.AppendLine($"Fecha: {venta.Fecha}");
            sb.AppendLine($"Ticket Nro: {venta.Id}");
            sb.AppendLine("--------------------------------");

            foreach (var item in venta.Detalles)
            {
                sb.AppendLine($"{item.Producto.Nombre}");
                sb.AppendLine($"{item.Cantidad} x ${item.PrecioUnitario} = ${item.Subtotal}");
            }

            sb.AppendLine("--------------------------------");
            sb.AppendLine($"TOTAL: ${venta.Total}");
            sb.AppendLine("================================");
            sb.AppendLine("    GRACIAS POR SU COMPRA       ");

            // Guardar en escritorio para probar
            string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Ticket_{venta.Id}.txt");
            File.WriteAllText(ruta, sb.ToString());
        }
    }
}