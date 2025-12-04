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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaintenanceId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public int MaintenanceType { get; set; } // 1 = Revisión por daño, 2 = Reparación mayor, 3 = Preventivo

        [Required]
        [Range(0, 2)]
        public int MaintenanceStatus { get; set; } = 0; // 0 = Pendiente, 1 = Finalizado, 2 = Cancelado (opcional)

        [Required]
        [ForeignKey(nameof(Equipment))]
        public int EquipmentId { get; set; }

        [StringLength(500)]
        public string Comments { get; set; }

        [Column(TypeName = "decimal")] 
        public decimal? Cost { get; set; }

        [StringLength(255)]
        public string EvidencePath { get; set; }

        [StringLength(256)]
        public string FinalizadoPor { get; set; }

        public virtual EquipmentDA Equipment { get; set; }


        public int? IdEmpleado { get; set; }


        public int? OrderId { get; set; }
        public virtual RentalOrderDA RentalOrder { get; set; }  // opcional, si quieres navegación


    }
}
