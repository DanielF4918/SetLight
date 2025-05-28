using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.AccesoADatos.Modelos
{
    [Table("Equipment")]
    public class EquipmentDA
    {
        [Key]
        [Column("EquipmentId")]
        public int EquipmentId { get; set; }

        [Column("EquipmentName")]
        public string EquipmentName { get; set; }

        [Column("Brand")]
        public string Brand { get; set; }

        [Column("Model")]
        public string Model { get; set; }

        [Column("SerialNumber")]
        public string SerialNumber { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("CategoryId")]
        public int CategoryId { get; set; }

        [Column("Status")]
        public int Status { get; set; }

        [ForeignKey("CategoryId")]
        public virtual EqCategoryDA Category { get; set; }
    }
}
