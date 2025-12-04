using System;
using System.Collections.Generic;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.Abstracciones.ViewModels
{
    public class CrearMaintenanceViewModel
    {
        public int EquipmentId { get; set; }          // ya no nullable, para simplificar
        public string Comments { get; set; }

        public int MaintenanceType { get; set; }

        public DateTime StartDate { get; set; }       // nueva
        public DateTime? EndDate { get; set; }        // puede ser null si no la pones aún

        public decimal? Cost { get; set; }            // nueva, si la usas en el form
        public int Cantidad { get; set; }             // nueva: nº de unidades a mandar a mantenimiento

        public string EquipmentName { get; set; }

        // Lista para el modal
        public List<EquipmentDto> Equipos { get; set; } = new List<EquipmentDto>();
    }
}
