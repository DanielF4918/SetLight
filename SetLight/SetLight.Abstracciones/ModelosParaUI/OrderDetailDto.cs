namespace SetLight.Abstracciones.ModelosParaUI
{
    public class OrderDetailDto
    {
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }

        // Precio actual (inventario) - opcional según uso
        public decimal RentalValue { get; set; }

        // ✅ Precio pactado (snapshot) guardado en OrderDetails.UnitRentalPrice
        public decimal UnitRentalPrice { get; set; }

        public int Quantity { get; set; }
        public int Stock { get; set; }

        public decimal? DescuentoManual { get; set; }
        public int CantidadMaxima { get; set; }
    }
}
