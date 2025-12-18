using System;
using System.Collections.Generic;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class RentalOrderDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusOrder { get; set; }
        public int ClientId { get; set; }
        public string RutaComprobante { get; set; }
        public int? EmpleadoId { get; set; }
        public string ClientName { get; set; }
        public List<OrderDetailDto> Details { get; set; } = new List<OrderDetailDto>();
        public string EmpleadoNombreCompleto { get; set; }
        public decimal? DescuentoManual { get; set; }

        public bool IsDelivery { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal TransportCost { get; set; }

        // Cálculos / resumen
        public int CantidadDias { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
    }
}
