using Entidades;
using System;
using System.Collections.Generic;
using System.Printing; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace puntoDeVenta.Services
{
    public class ImpresionService
    {
        private const double ANCHO_PAGINA = 280; 
        private readonly FontFamily FUENTE_TICKET = new FontFamily("Consolas");

        public void ImprimirTicket(Venta venta, List<DetalleVenta> detalles, decimal pagoCon, decimal cambio, Configuracion config)
        {
           
            FlowDocument doc = CrearDocumentoVisual(venta, detalles, pagoCon, cambio, config);

            // 2. IMPRESIÓN SILENCIOSA
            try
            {
                PrintDialog pd = new PrintDialog();

                if (!string.IsNullOrEmpty(config.NombreImpresora))
                {
                   
                    pd.PrintQueue = new LocalPrintServer().GetPrintQueue(config.NombreImpresora);
                }

                
                doc.PageWidth = pd.PrintableAreaWidth;
                if (doc.PageWidth <= 0) doc.PageWidth = ANCHO_PAGINA; // Fallback por si falla la detección

                // Enviamos directo sin abrir ventana (ShowDialog)
                pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, $"Ticket #{venta.Id}");
            }
            catch (Exception)
            {
                
                MessageBox.Show("No se encontró la impresora configurada. Seleccione una manualmente.", "Aviso Impresión");
                PrintDialog pdManual = new PrintDialog();
                if (pdManual.ShowDialog() == true)
                {
                    doc.PageWidth = pdManual.PrintableAreaWidth;
                    pdManual.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, $"Ticket #{venta.Id}");
                }
            }
        }

        // Método auxiliar que genera el diseño visual (Separado para limpieza)
        private FlowDocument CrearDocumentoVisual(Venta venta, List<DetalleVenta> detalles, decimal pagoCon, decimal cambio, Configuracion config)
        {
            FlowDocument doc = new FlowDocument();
            doc.PagePadding = new Thickness(2); 
            doc.ColumnWidth = ANCHO_PAGINA;
            doc.FontFamily = FUENTE_TICKET;
            doc.FontSize = 10;

            // --- ENCABEZADO ---
            Paragraph header = new Paragraph();
            header.TextAlignment = TextAlignment.Center;
            header.Inlines.Add(new Run(config.NombreNegocio?.ToUpper() ?? "MI NEGOCIO") { FontSize = 14, FontWeight = FontWeights.Bold });
            header.Inlines.Add(new LineBreak());
            if (!string.IsNullOrEmpty(config.Direccion))
            {
                header.Inlines.Add(new Run(config.Direccion) { FontSize = 9 });
                header.Inlines.Add(new LineBreak());
            }
            header.Inlines.Add(new Run("--------------------------------") { FontSize = 8 });
            doc.Blocks.Add(header);

            // --- DATOS ---
            Paragraph meta = new Paragraph();
            meta.FontSize = 9;
            meta.Inlines.Add(new Run($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}"));
            meta.Inlines.Add(new LineBreak());
            meta.Inlines.Add(new Run($"Ticket Nro: {venta.Id}"));
            doc.Blocks.Add(meta);

            // --- ITEMS ---
            Table tabla = new Table();
            tabla.CellSpacing = 0;
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(30) }); // Cant
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(140) }); // Prod
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(60) }); // Total

            TableRowGroup grupo = new TableRowGroup();
            foreach (var item in detalles)
            {
                TableRow row = new TableRow();
                row.Cells.Add(CrearCelda(item.Cantidad.ToString(), TextAlignment.Left));
                row.Cells.Add(CrearCelda(item.Producto.Nombre, TextAlignment.Left)); // El nombre largo baja solo
                row.Cells.Add(CrearCelda($"${item.Subtotal:N2}", TextAlignment.Right));
                grupo.Rows.Add(row);
            }
            tabla.RowGroups.Add(grupo);
            doc.Blocks.Add(tabla);

            // --- TOTALES ---
            Paragraph totalP = new Paragraph();
            totalP.TextAlignment = TextAlignment.Right;
            totalP.Inlines.Add(new Run("--------------------------------"));
            totalP.Inlines.Add(new LineBreak());
            totalP.Inlines.Add(new Run($"TOTAL: ${venta.Total:N2}") { FontSize = 16, FontWeight = FontWeights.Bold });
            totalP.Inlines.Add(new LineBreak());
            totalP.Inlines.Add(new Run($"Efectivo: ${pagoCon:N2}") { FontSize = 9 });
            totalP.Inlines.Add(new LineBreak());
            totalP.Inlines.Add(new Run($"Su Vuelto: ${cambio:N2}") { FontSize = 9 });
            doc.Blocks.Add(totalP);

            // --- PIE ---
            Paragraph footer = new Paragraph(new Run("¡Gracias por su compra!"));
            footer.TextAlignment = TextAlignment.Center;
            footer.FontSize = 9;
            footer.Margin = new Thickness(0, 10, 0, 0);
            doc.Blocks.Add(footer);

            return doc;
        }

        private TableCell CrearCelda(string texto, TextAlignment align)
        {
            return new TableCell(new Paragraph(new Run(texto)) { TextAlignment = align, Margin = new Thickness(0) });
        }
    }
}