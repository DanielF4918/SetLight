using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SetLight.AccesoADatos.Modelos
{
    [Table("RentalOrders")]
    public class RentalOrderDA
    {
        [Key]
        [Column("OrderId")]
        public int OrderId { get; set; }

        [Column("OrderDate")]
        public DateTime OrderDate { get; set; }

        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Column("EndDate")]
        public DateTime EndDate { get; set; }

        [Column("StatusOrder")]
        public int StatusOrder { get; set; }

        [Column("ClientId")]
        public int ClientId { get; set; }
    }
}
