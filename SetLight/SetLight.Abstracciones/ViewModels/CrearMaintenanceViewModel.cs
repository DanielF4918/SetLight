using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.Abstracciones.ViewModels
{
    public class CrearMaintenanceViewModel
    {
        public int? EquipmentId { get; set; }
        public string Comments { get; set; }

        public int MaintenanceType { get; set; }

        public string EquipmentName { get; set; }

        // Lista para el modal
        public List<EquipmentDto> Equipos { get; set; } = new List<EquipmentDto>();
    }
}
