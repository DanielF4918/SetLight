using System.Threading.Tasks;
using System;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.CrearRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Modelos;
using System.Linq;
using System.Data.Entity; // si usas EF6

public class CrearRentalOrderAD : ICrearRentalOrderAD
{
    public async Task<int> Guardar(RentalOrderDto orden)
    {
        using (var db = new Contexto())
        using (var tx = db.Database.BeginTransaction())
        {
            try
            {
                // (Opcional defensivo) si EmpleadoId es obligatorio, bloquea aquí:
                // if (!orden.EmpleadoId.HasValue) throw new InvalidOperationException("Empleado inválido.");

                var entidad = new RentalOrderDA
                {
                    OrderDate = DateTime.Now,
                    StartDate = orden.StartDate,
                    EndDate = orden.EndDate,
                    ClientId = orden.ClientId,
                    StatusOrder = orden.StatusOrder,
                    EmpleadoId = orden.EmpleadoId,
                    DescuentoManual = orden.DescuentoManual,
                    RutaComprobante = orden.RutaComprobante
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

                    db.OrderDetails.Add(new OrderDetailDA
                    {
                        OrderId = entidad.OrderId,
                        EquipmentId = detalle.EquipmentId,
                        Quantity = detalle.Quantity
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
