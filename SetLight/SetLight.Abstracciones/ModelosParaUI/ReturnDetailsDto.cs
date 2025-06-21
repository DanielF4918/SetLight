using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class ReturnDetailsDto
    {
        public int ReturnDetailId { get; set; }
        public int OrderId { get; set; }
        public int EquipmentId { get; set; }
        public DateTime ReturnDate { get; set; }
        public string ConditionReport { get; set; }
        public bool IsReturned { get; set; }
        public bool RequiresMaintenance { get; set; }

        public string EquipmentName { get; set; }
        public string RentalOrder  { get; set; }
    }
}
