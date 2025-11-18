using System;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class TrazabilidadDto
    {
        public int EquipmentId { get; set; }
        public string EquipmentNombre { get; set; }
        public string TipoEvento { get; set; }

        public int? OrderId { get; set; }
        public int? MaintenanceId { get; set; }

        //  Préstamos
        public string ClienteNombre { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string EncargadoPrestamo { get; set; }

        //  Mantenimientos
        public DateTime? FechaMantenimiento { get; set; }
        public int TipoMantenimiento { get; set; }
        public string Tecnico { get; set; }
        public string Comentarios { get; set; }
    }
}
