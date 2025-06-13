using System;
using System.Collections.Generic;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.Abstracciones.ViewModels
{
    public class CrearRentalOrderViewModel
    {
        public int ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusOrder { get; set; }

        public List<ClientDto> Clientes { get; set; } = new List<ClientDto>();
        public List<OrderDetailDto> EquiposDisponibles { get; set; } = new List<OrderDetailDto>();
        public List<OrderDetailDto> EquiposSeleccionados { get; set; } = new List<OrderDetailDto>();

    }
}
