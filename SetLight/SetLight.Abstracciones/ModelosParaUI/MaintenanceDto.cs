using System;

namespace SetLight.Entidades.Dto
{
    public class MaintenanceDto
    {
        public int MaintenanceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int MaintenanceType { get; set; }
        public int MaintenanceStatus { get; set; }
        public int EquipmentId { get; set; }

        public string Comments { get; set; }
        public decimal? Cost { get; set; }
        public string EvidencePath { get; set; }

        public string EquipmentName { get; set; }

        // 🔹 NUEVO: técnico responsable
        public int? IdEmpleado { get; set; }
        public string TechnicianName { get; set; }

        // (Opcional, solo si quieres usar también FinalizadoPor en la vista)
        public string FinalizadoPor { get; set; }
    }
}
