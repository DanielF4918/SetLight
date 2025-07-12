using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.Entidades
{
    [Table("Maintenance")]
    public class Maintenance
    {
        [Key]
        public int MaintenanceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public string MaintenanceType { get; set; }

        [Required]
        public int MaintenanceStatus { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [ForeignKey("EquipmentId")]
        public virtual EquipmentDA Equipment { get; set; }
    }
}
