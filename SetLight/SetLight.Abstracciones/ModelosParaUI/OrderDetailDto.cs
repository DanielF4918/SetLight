using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class OrderDetailDto
    {
        public int OrderId { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public decimal RentalValue { get; set; }
        public int Quantity { get; set; }


        public int Stock { get; set; }
    }
}
