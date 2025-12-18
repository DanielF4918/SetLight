using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.Abstracciones.ViewModels
{
    public class CrearRentalOrderViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Debe ingresar la fecha de inicio.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Debe ingresar la fecha de fin.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int StatusOrder { get; set; }

        public List<ClientDto> Clientes { get; set; } = new List<ClientDto>();
        public List<OrderDetailDto> EquiposDisponibles { get; set; } = new List<OrderDetailDto>();
        public List<OrderDetailDto> EquiposSeleccionados { get; set; } = new List<OrderDetailDto>();

        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo.")]
        public decimal? DescuentoManual { get; set; }



        [Display(Name = "¿Con entrega?")]
        public bool IsDelivery { get; set; } = false;

        [Display(Name = "Dirección de entrega")]
        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres.")]
        public string DeliveryAddress { get; set; }

        [Display(Name = "Costo de transporte")]
        [Range(0, 9999999999999999.99, ErrorMessage = "El costo de transporte no puede ser negativo.")]
        public decimal TransportCost { get; set; } = 0m;
    }
}
