using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class EquipmentDto
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int CategoryId { get; set; }
    }
}
