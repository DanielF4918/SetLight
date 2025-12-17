using System;
using System.ComponentModel.DataAnnotations;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class ReturnDetailsDto
    {
        public int ReturnDetailId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int EquipmentId { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        public string ConditionReport { get; set; }

        public bool IsReturned { get; set; }

        public bool RequiresMaintenance { get; set; }

        public string EquipmentName { get; set; }

        public string RentalOrder { get; set; }

        public int ClientId { get; set; }
        public string ClientName { get; set; }

        // ✅ Precio pactado (snapshot) para esa orden/equipo
        public decimal UnitRentalPrice { get; set; }
    }
}
