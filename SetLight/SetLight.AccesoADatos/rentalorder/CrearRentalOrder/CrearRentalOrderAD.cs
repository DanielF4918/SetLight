using System.Threading.Tasks;
using System;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.CrearRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Modelos;
using System.Linq;

public class CrearRentalOrderAD : ICrearRentalOrderAD
{
    public async Task<int> Guardar(RentalOrderDto orden)
    {
        using (var db = new Contexto())
        {
            var entidad = new RentalOrderDA
            {
                OrderDate = DateTime.Now,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                ClientId = orden.ClientId,
                StatusOrder = orden.StatusOrder
            };

            db.RentalOrders.Add(entidad);
            await db.SaveChangesAsync();

            foreach (var detalle in orden.Details)
            {
                db.OrderDetails.Add(new OrderDetailDA
                {
                    OrderId = entidad.OrderId,
                    EquipmentId = detalle.EquipmentId,
                    Quantity = detalle.Quantity
                });

                var equipo = db.Equipment.FirstOrDefault(e => e.EquipmentId == detalle.EquipmentId);
                if (equipo != null)
                {
                }
            }

            await db.SaveChangesAsync();
            return entidad.OrderId;
        }
    }
}
