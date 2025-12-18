using System;
using System.Linq;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.CrearRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Modelos;
using System.Data.Entity; // EF6

public class CrearRentalOrderAD : ICrearRentalOrderAD
{
    public async Task<int> Guardar(RentalOrderDto orden)
    {
        using (var db = new Contexto())
        using (var tx = db.Database.BeginTransaction())
        {
            try
            {
                // =======================
                // ✅ Normalización defensiva (Misión 2)
                // =======================
                bool isDelivery = orden.IsDelivery;

                // Si no hay entrega: limpiamos valores para evitar basura
                string deliveryAddress = isDelivery
                    ? (orden.DeliveryAddress ?? "").Trim()
                    : null;

                decimal transportCost = isDelivery
                    ? orden.TransportCost
                    : 0m;

                if (transportCost < 0) transportCost = 0m; // blindaje extra

                if (isDelivery && string.IsNullOrWhiteSpace(deliveryAddress))
                    throw new InvalidOperationException("La dirección de entrega es obligatoria cuando la orden es con entrega.");

                // =======================
                // Crear entidad Order
                // =======================
                var entidad = new RentalOrderDA
                {
                    OrderDate = DateTime.Now,
                    StartDate = orden.StartDate,
                    EndDate = orden.EndDate,
                    ClientId = orden.ClientId,
                    StatusOrder = orden.StatusOrder,
                    EmpleadoId = orden.EmpleadoId,
                    DescuentoManual = orden.DescuentoManual,
                    RutaComprobante = orden.RutaComprobante,

                    // =======================
                    // ✅ Misión 2: mapear columnas nuevas
                    // =======================
                    IsDelivery = isDelivery,
                    DeliveryAddress = deliveryAddress,
                    TransportCost = transportCost
                };

                db.RentalOrders.Add(entidad);

                // Guardamos para obtener OrderId (IDENTITY)
                await db.SaveChangesAsync();

                foreach (var detalle in orden.Details)
                {
                    var equipo = db.Equipment.FirstOrDefault(e => e.EquipmentId == detalle.EquipmentId);

                    if (equipo == null)
                        throw new InvalidOperationException("El equipo seleccionado no existe.");

                    if (equipo.Stock < detalle.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"No hay suficiente stock para el equipo: {equipo.EquipmentName}. " +
                            $"Disponible: {equipo.Stock}, solicitado: {detalle.Quantity}."
                        );
                    }

                    // ✅ Precio pactado (snapshot)
                    // Preferimos el valor que viene en el DTO (ya congelado desde BD en el controller),
                    // y si viene en 0, caemos al RentalValue actual como respaldo defensivo.
                    var precioPactado = (detalle.RentalValue > 0m)
                        ? detalle.RentalValue
                        : equipo.RentalValue;

                    db.OrderDetails.Add(new OrderDetailDA
                    {
                        OrderId = entidad.OrderId,
                        EquipmentId = detalle.EquipmentId,
                        Quantity = detalle.Quantity,

                        // ✅ Guardar precio pactado
                        UnitRentalPrice = precioPactado
                    });

                    equipo.Stock -= detalle.Quantity;

                    if (equipo.Stock <= 0)
                    {
                        equipo.Stock = 0;
                        equipo.Status = 2; // Agotado / Sin stock
                    }
                }

                await db.SaveChangesAsync();
                tx.Commit();

                return entidad.OrderId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
