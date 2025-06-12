using SetLight.Abstracciones.ModelosParaUI;
using System.Collections.Generic;
using System;

public class CrearRentalOrderViewModel
{
    public int ClientId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int StatusOrder { get; set; }
    public List<OrderDetailDto> EquiposDisponibles { get; set; } = new List<OrderDetailDto>();
    public List<ClientDto> Clientes { get; set; }


}
