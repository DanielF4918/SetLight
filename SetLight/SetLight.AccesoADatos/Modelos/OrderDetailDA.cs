using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SetLight.AccesoADatos.Modelos
{
    [Table("OrderDetails")]
    public class OrderDetailDA
    {
        [Key]
        [Column("DetailId")]
        public int DetailId { get; set; }

        [Column("OrderId")]
        public int OrderId { get; set; }

        [Column("EquipmentId")]
        public int EquipmentId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }
    }
}
