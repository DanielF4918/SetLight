using System;
using System.Globalization; // USD
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.LogicaDeNegocio.Services
{
    public static class ComprobantePdfService
    {
        public static byte[] GenerarEnMemoria(RentalOrderDto orden)
        {
            using (var stream = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, stream);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var tableBodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // Cultura USD
                var us = CultureInfo.GetCultureInfo("en-US");

                // Título
                Paragraph title = new Paragraph("ORDEN DE ALQUILER", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                doc.Add(title);

                // Datos empresa y cliente
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1, 1 });

                PdfPCell left = new PdfPCell { Border = Rectangle.NO_BORDER };
                left.AddElement(new Paragraph("Emitido por:", labelFont));
                left.AddElement(new Paragraph("Light Project Films", valueFont));
                left.AddElement(new Paragraph("San José, Costa Rica", valueFont));
                left.AddElement(new Paragraph("Tel: 2222-0000", valueFont));

                PdfPCell right = new PdfPCell { Border = Rectangle.NO_BORDER };
                right.AddElement(new Paragraph("Cliente:", labelFont));
                right.AddElement(new Paragraph(orden.ClientName, valueFont));
                right.AddElement(new Paragraph("Orden ID: " + orden.OrderId, valueFont));
                right.AddElement(new Paragraph("Fecha: " + orden.OrderDate.ToShortDateString(), valueFont));

                headerTable.AddCell(left);
                headerTable.AddCell(right);
                doc.Add(headerTable);

                doc.Add(new Paragraph("\n"));

                // =========================
                // ✅ SECCIÓN ENTREGA
                // =========================
                PdfPTable entregaTable = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    SpacingAfter = 10
                };
                entregaTable.SetWidths(new float[] { 1, 2 });

                void AddEntregaRow(string label, string value)
                {
                    entregaTable.AddCell(new PdfPCell(new Phrase(label, tableHeaderFont))
                    {
                        Border = Rectangle.NO_BORDER
                    });

                    entregaTable.AddCell(new PdfPCell(new Phrase(value ?? "", tableBodyFont))
                    {
                        Border = Rectangle.NO_BORDER
                    });
                }

                var esEntrega = orden.IsDelivery;
                var direccion = string.IsNullOrWhiteSpace(orden.DeliveryAddress) ? "No aplica" : orden.DeliveryAddress;

                // TransportCost en tu DB es NOT NULL, así que asumimos decimal válido
                var costoTransporte = esEntrega ? orden.TransportCost : 0m;

                AddEntregaRow("Entrega a domicilio:", esEntrega ? "Sí" : "No");
                AddEntregaRow("Dirección:", esEntrega ? direccion : "No aplica");
                AddEntregaRow("Costo transporte:", esEntrega ? costoTransporte.ToString("C2", us) : "No aplica");

                doc.Add(entregaTable);

                // =========================
                // TABLA DE EQUIPOS
                // =========================
                PdfPTable equipoTable = new PdfPTable(6) { WidthPercentage = 100 };
                equipoTable.SetWidths(new float[] { 3, 2, 2, 1, 2, 2 });

                string[] headers = { "Equipo", "Marca", "Modelo", "Cant.", "Precio Unitario", "Subtotal" };
                foreach (string h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, tableHeaderFont))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    equipoTable.AddCell(cell);
                }

                // Cálculo por días de alquiler
                int cantidadDias = (orden.EndDate - orden.StartDate).Days + 1;
                if (cantidadDias < 1) cantidadDias = 1;

                decimal subtotalEquipos = 0m;

                foreach (var item in orden.Details)
                {
                    decimal subtotalItem = item.RentalValue * item.Quantity * cantidadDias;
                    subtotalEquipos += subtotalItem;

                    equipoTable.AddCell(new Phrase(item.EquipmentName, tableBodyFont));
                    equipoTable.AddCell(new Phrase(item.Brand, tableBodyFont));
                    equipoTable.AddCell(new Phrase(item.Model, tableBodyFont));
                    equipoTable.AddCell(new Phrase(item.Quantity.ToString(), tableBodyFont));
                    equipoTable.AddCell(new Phrase(item.RentalValue.ToString("C2", us), tableBodyFont)); // USD
                    equipoTable.AddCell(new Phrase(subtotalItem.ToString("C2", us), tableBodyFont));      // USD
                }

                doc.Add(equipoTable);
                doc.Add(new Paragraph("\n"));

                // =========================
                // ✅ CÁLCULOS (igual modal/JS):
                // subtotal equipos + transporte => IVA => descuento => total
                // =========================
                decimal subtotalConTransporte = subtotalEquipos + costoTransporte;

                decimal iva = Math.Round(subtotalConTransporte * 0.13m, 2);
                decimal totalBruto = subtotalConTransporte + iva;

                decimal porcentajeDescuento = orden.DescuentoManual ?? 0m;
                if (porcentajeDescuento < 0m) porcentajeDescuento = 0m;
                if (porcentajeDescuento > 100m) porcentajeDescuento = 100m;

                decimal montoDescuento = Math.Round(totalBruto * (porcentajeDescuento / 100m), 2);
                decimal totalFinal = totalBruto - montoDescuento;

                // =========================
                // RESUMEN
                // =========================
                PdfPTable resumen = new PdfPTable(2)
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    WidthPercentage = 45,
                    SpacingBefore = 10
                };
                resumen.SetWidths(new float[] { 1, 1 });

                void AddResumenRow(string label, string value, bool bold = false)
                {
                    resumen.AddCell(new PdfPCell(new Phrase(label, bold ? tableHeaderFont : tableBodyFont))
                    {
                        Border = Rectangle.NO_BORDER
                    });

                    resumen.AddCell(new PdfPCell(new Phrase(value, bold ? tableHeaderFont : tableBodyFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });
                }

                AddResumenRow("DÍAS DE ALQUILER", cantidadDias.ToString());
                AddResumenRow("SUBTOTAL EQUIPOS", subtotalEquipos.ToString("C2", us));

                // Transporte como línea siempre, pero en 0 si no aplica
                AddResumenRow("TRANSPORTE", costoTransporte.ToString("C2", us));

                AddResumenRow("SUBTOTAL (CON TRANSP.)", subtotalConTransporte.ToString("C2", us));
                AddResumenRow("IVA (13%)", iva.ToString("C2", us));

                // Mostrar % con 0-2 decimales
                AddResumenRow($"DESCUENTO ({porcentajeDescuento.ToString("N2")}%)", "-" + montoDescuento.ToString("C2", us));

                AddResumenRow("TOTAL A PAGAR", totalFinal.ToString("C2", us), true);

                doc.Add(resumen);

                // Firma
                doc.Add(new Paragraph("\n\nNotas: El cliente es responsable por el uso adecuado del equipo durante el periodo de alquiler.\n\n", valueFont));
                doc.Add(new Paragraph("____________________________", valueFont));
                doc.Add(new Paragraph("Firma de la Empresa", valueFont));

                doc.Close();
                return stream.ToArray();
            }
        }
    }
}
