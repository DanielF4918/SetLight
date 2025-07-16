using System.Collections.Generic;
using System.IO;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.LogicaDeNegocio.Trazabilidad
{
    public class TrazabilidadPdfService
    {
        public string Generar(int equipoId, List<TrazabilidadDto> eventos)
        {
            string folderPath = HttpContext.Current.Server.MapPath("~/TrazabilidadPDF/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"Trazabilidad_Equipo_{equipoId}.pdf");

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (Document doc = new Document())
            using (PdfWriter writer = PdfWriter.GetInstance(doc, fs))
            {
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                doc.Add(new Paragraph($"Trazabilidad del Equipo #{equipoId}", titleFont));
                doc.Add(new Paragraph("\n"));

                PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
                table.AddCell("Tipo de Evento");
                table.AddCell("Fecha Inicio");
                table.AddCell("Fecha Fin / Mantenimiento");
                table.AddCell("Persona / Técnico");
                table.AddCell("Tipo de Mantenimiento");

                foreach (var item in eventos)
                {
                    table.AddCell(item.TipoEvento);
                    table.AddCell(item.FechaInicio?.ToString("dd/MM/yyyy") ?? "-");
                    table.AddCell(item.FechaFin?.ToString("dd/MM/yyyy") ?? item.FechaMantenimiento?.ToString("dd/MM/yyyy") ?? "-");
                    table.AddCell(item.ClienteNombre ?? item.Tecnico ?? "-");
                    table.AddCell(item.TipoEvento == "Mantenimiento" ? item.TipoMantenimiento.ToString() : "-");
                }

                doc.Add(table);
                doc.Close();
            }

            return filePath;
        }
    }
}
