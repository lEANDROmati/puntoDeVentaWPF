using Entidades;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace puntoDeVenta.Services
{
    public class ImpresionService
    {
        // AJUSTES DE IMPRESORA 80mm
        // El ancho imprimible suele ser ~280-300px.
        private const double ANCHO_PAGINA = 300;
        private readonly FontFamily FUENTE_TICKET = new FontFamily("Consolas"); // Fuente monoespaciada para que todo encaje

        public void ImprimirTicket(Venta venta, List<DetalleVenta> detalles, decimal pagoCon, decimal cambio, string nombreNegocio, string direccion, string telefono)
        {
            // 1. Configurar el Documento (El "Papel Virtual")
            FlowDocument doc = new FlowDocument();
            doc.PagePadding = new Thickness(5);
            doc.ColumnWidth = ANCHO_PAGINA;
            doc.FontFamily = FUENTE_TICKET;
            doc.FontSize = 10; // Tamaño base legible

            // ==========================================
            // 1. ENCABEZADO (Branding)
            // ==========================================
            Paragraph header = new Paragraph();
            header.TextAlignment = TextAlignment.Center;

            // Nombre del Negocio (Grande y Negrita)
            header.Inlines.Add(new Run(nombreNegocio.ToUpper())
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold
            });
            header.Inlines.Add(new LineBreak());

            // Dirección y Contacto (Más chico)
            if (!string.IsNullOrEmpty(direccion))
            {
                header.Inlines.Add(new Run(direccion) { FontSize = 9 });
                header.Inlines.Add(new LineBreak());
            }
            if (!string.IsNullOrEmpty(telefono))
            {
                header.Inlines.Add(new Run($"Tel/Wpp: {telefono}") { FontSize = 9 });
            }
            doc.Blocks.Add(header);

            doc.Blocks.Add(CrearSeparador());

            // ==========================================
            // 2. DATOS DE LA VENTA (Contexto)
            // ==========================================
            Paragraph metaData = new Paragraph();
            metaData.FontSize = 9;
            metaData.Inlines.Add(new Run($"Fecha: {DateTime.Now:dd/MM/yyyy}  Hora: {DateTime.Now:HH:mm}"));
            metaData.Inlines.Add(new LineBreak());
            metaData.Inlines.Add(new Run($"Ticket #: {venta.Id.ToString().PadLeft(6, '0')}"));
            // metaData.Inlines.Add(new Run($"Cajero: Admin")); // Descomentar si tienes usuarios
            doc.Blocks.Add(metaData);

            doc.Blocks.Add(CrearSeparador());

            // ==========================================
            // 3. TABLA DE PRODUCTOS (Cuerpo)
            // ==========================================
            Table tabla = new Table();
            tabla.CellSpacing = 0;
            // Definimos columnas: 

            // Columna 1: CANT (Chica, para 2 o 3 dígitos)
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(40) });

            // Columna 2: PRODUCTO (Grande, le damos 170px fijos para que entre texto cómodo)
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(170) });

            // Columna 3: TOTAL (Mediana, para precios largos)
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(80) });

            // Cabecera de la Tabla
            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(CrearCelda("CANT", TextAlignment.Center, true));
            headerRow.Cells.Add(CrearCelda("PRODUCTO", TextAlignment.Left, true));
            headerRow.Cells.Add(CrearCelda("TOTAL", TextAlignment.Right, true));
            headerGroup.Rows.Add(headerRow);
            tabla.RowGroups.Add(headerGroup);

            // Filas de Productos
            TableRowGroup bodyGroup = new TableRowGroup();
            foreach (var item in detalles)
            {
                TableRow row = new TableRow();
                // Cantidad
                row.Cells.Add(CrearCelda(item.Cantidad.ToString(), TextAlignment.Center));

                // Nombre (El FlowDocument se encarga de hacer el "Wrap" si es largo)
                row.Cells.Add(CrearCelda(item.Producto.Nombre, TextAlignment.Left));

                // Precio Total del item
                row.Cells.Add(CrearCelda($"${item.Subtotal:N2}", TextAlignment.Right));

                bodyGroup.Rows.Add(row);
            }
            tabla.RowGroups.Add(bodyGroup);
            doc.Blocks.Add(tabla);

            doc.Blocks.Add(CrearSeparador());

            // ==========================================
            // 4. TOTALES (Jerarquía Visual Alta)
            // ==========================================
            Paragraph totales = new Paragraph();
            totales.TextAlignment = TextAlignment.Center;

            // 1. Etiqueta "TOTAL" (Tamaño medio)
            totales.Inlines.Add(new Run("TOTAL A PAGAR")
            {
                FontSize = 12,
                FontWeight = FontWeights.Bold
            });
            totales.Inlines.Add(new LineBreak());

            // 2. El MONTO (Gigante)
            // Al estar en su propia línea, puede ser enorme sin romper nada
            totales.Inlines.Add(new Run($"${venta.Total:N2}")
            {
                FontSize = 22,
                FontWeight = FontWeights.Black
            });

            totales.Inlines.Add(new LineBreak());

            // 3. Detalles de pago (Más chicos y también centrados)
            totales.Inlines.Add(new Run($"Efectivo: ${pagoCon:N2}   Vuelto: ${cambio:N2}")
            {
                FontSize = 10
            });

            doc.Blocks.Add(totales);

            // Separador final
            doc.Blocks.Add(CrearSeparador());

            // ==========================================
            // 5. PIE DE PÁGINA (Legales y Marketing)
            // ==========================================
            Paragraph footer = new Paragraph();
            footer.TextAlignment = TextAlignment.Center;
            footer.FontSize = 9;

            // Aviso Legal Importante
            footer.Inlines.Add(new Run("*** NO VÁLIDO COMO FACTURA ***") { FontWeight = FontWeights.Bold });
            footer.Inlines.Add(new LineBreak());

            footer.Inlines.Add(new Run("¡Gracias por su compra!"));
            footer.Inlines.Add(new LineBreak());

            // Marketing / Wifi
            // footer.Inlines.Add(new Run("Wifi: Clientes2025")); 

            doc.Blocks.Add(footer);

            // ==========================================
            // 6. ENVIAR A IMPRESORA
            // ==========================================
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Ajustamos el ancho del papel al de la impresora real seleccionada
                doc.PageWidth = printDialog.PrintableAreaWidth;

                // Imprimimos
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, $"Ticket Venta #{venta.Id}");
            }
        }

        // --- MÉTODOS AYUDANTES PARA NO REPETIR CÓDIGO ---

        // Crea una línea de guiones
        private Paragraph CrearSeparador()
        {
            var p = new Paragraph(new Run("--------------------------------------------------"));
            p.TextAlignment = TextAlignment.Center;
            p.Margin = new Thickness(0, 5, 0, 5); // Un poco de aire arriba y abajo
            return p;
        }

        // Crea una celda para la tabla
        private TableCell CrearCelda(string texto, TextAlignment alineacion, bool esNegrita = false)
        {
            var run = new Run(texto);
            if (esNegrita) run.FontWeight = FontWeights.Bold;

            var p = new Paragraph(run);
            p.TextAlignment = alineacion;
            p.Margin = new Thickness(0, 2, 0, 2); // Separación entre filas

            return new TableCell(p);
        }
    }
}