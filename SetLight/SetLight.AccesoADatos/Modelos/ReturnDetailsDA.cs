using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SetLight.AccesoADatos.Modelos
{
    [Table("ReturnDetails")]
    public class ReturnDetailDa
    {
        [Key]
        [Column("ReturnDetailId")]
        public int ReturnDetailId { get; set; }

        [Column("OrderId")]
        public int OrderId { get; set; }

        [Column("EquipmentId")]
        public int EquipmentId { get; set; }

        [Column("ReturnDate")]
        public DateTime ReturnDate { get; set; }

        [Column("ConditionReport")]
        public string ConditionReport { get; set; }

        [Column("IsReturned")]
        public bool IsReturned { get; set; }

        [Column("RequiresMaintenance")]
        public bool RequiresMaintenance { get; set; }


        public virtual EquipmentDA Equipment { get; set; }

    }
}
