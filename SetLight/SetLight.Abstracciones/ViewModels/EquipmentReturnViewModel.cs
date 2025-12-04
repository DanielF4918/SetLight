using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.ViewModels
{
    public class EquipmentReturnViewModel
    {
        public int OrderId { get; set; }
        public string ClientName { get; set; }
        public List<EquipmentReturnItem> Items { get; set; }
    }

    public class EquipmentReturnItem
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public bool IsReturned { get; set; }
        public int CantidadBuenas { get; set; }
        public int CantidadDañadas { get; set; }
        public string Observaciones { get; set; }

        public int Quantity { get; set; }
        public int? MaintenanceType { get; set; }

        public int CantidadFaltante { get; set; }

    }

}
