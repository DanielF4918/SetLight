using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SetLight.Abstracciones.ModelosParaUI;
using System.Collections.Generic;

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

                // Tabla de equipos
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

                // 🔥 Cálculo por días de alquiler
                int cantidadDias = (orden.EndDate - orden.StartDate).Days + 1;
                decimal total = 0;
                foreach (var item in orden.Details)
                {
                    decimal subtotal = item.RentalValue * item.Quantity * cantidadDias;
                    total += subtotal;

                    equipoTable.AddCell(new Phrase(item.EquipmentName, tableBodyFont));
                    equipoTable.AddCell(new Phrase(item.Brand, tableBodyFont));
                    equipoTable.AddCell(new Phrase(item.Model, tableBodyFont));
                    equipoTable.AddCell(new Phrase(item.Quantity.ToString(), tableBodyFont));
                    equipoTable.AddCell(new Phrase("₡" + item.RentalValue.ToString("N2"), tableBodyFont));
                    equipoTable.AddCell(new Phrase("₡" + subtotal.ToString("N2"), tableBodyFont));
                }

                doc.Add(equipoTable);
                doc.Add(new Paragraph("\n"));

                // Cálculos con descuento como porcentaje
                decimal porcentajeDescuento = orden.DescuentoManual ?? 0;
                decimal montoDescuento = total * (porcentajeDescuento / 100m);
                decimal totalConDescuento = total - montoDescuento;
                decimal impuestos = totalConDescuento * 0.13m;
                decimal totalFinal = totalConDescuento + impuestos;

                // Resumen
                PdfPTable resumen = new PdfPTable(2)
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    WidthPercentage = 40,
                    SpacingBefore = 10
                };
                resumen.SetWidths(new float[] { 1, 1 });

                void AddResumenRow(string label, string value, bool bold = false)
                {
                    resumen.AddCell(new PdfPCell(new Phrase(label, bold ? tableHeaderFont : tableBodyFont)) { Border = Rectangle.NO_BORDER });
                    resumen.AddCell(new PdfPCell(new Phrase(value, bold ? tableHeaderFont : tableBodyFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });
                }

                resumen.AddCell(new PdfPCell(new Phrase("DÍAS DE ALQUILER", tableBodyFont)) { Border = Rectangle.NO_BORDER });
                resumen.AddCell(new PdfPCell(new Phrase(cantidadDias.ToString(), tableBodyFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                AddResumenRow("SUBTOTAL", "₡" + total.ToString("N2"));
                AddResumenRow($"DESCUENTO ({porcentajeDescuento:N0}%)", "-₡" + montoDescuento.ToString("N2"));
                AddResumenRow("IVA (13%)", "₡" + impuestos.ToString("N2"));
                AddResumenRow("TOTAL A PAGAR", "₡" + totalFinal.ToString("N2"), true);

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
