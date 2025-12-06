using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.EditRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.rentalorder.EditRentalOrder
{
    public class EditRentalOrderAD : IEditRentalOrderAD
    {
        public int Editar(RentalOrderDto ordenActualizada)
        {
            using (var db = new Contexto())
            {
                // 🧾 Traer la orden de la BD
                var ordenBD = db.RentalOrders
                    .FirstOrDefault(o => o.OrderId == ordenActualizada.OrderId);

                if (ordenBD == null)
                {
                    throw new InvalidOperationException("La orden que intenta editar no existe.");
                }

                // 🔁 Traer los detalles actuales de la orden
                var detallesActuales = db.OrderDetails
                    .Where(d => d.OrderId == ordenBD.OrderId)
                    .ToList();

                // 1) Devolver stock de los detalles actuales
                foreach (var det in detallesActuales)
                {
                    var equipo = db.Equipment.FirstOrDefault(e => e.EquipmentId == det.EquipmentId);
                    if (equipo != null)
                    {
                        equipo.Stock += det.Quantity;

                        // Si el equipo estaba marcado como sin stock (2) y ahora tiene, lo podemos marcar como disponible (1)
                        if (equipo.Stock > 0 && equipo.Status == 2)
                        {
                            equipo.Status = 1;
                        }
                    }
                }

                // 2) Eliminar los detalles actuales (dejamos limpia la orden)
                db.OrderDetails.RemoveRange(detallesActuales);
                db.SaveChanges();

                // 3) Actualizar cabecera de la orden
                ordenBD.ClientId = ordenActualizada.ClientId;
                ordenBD.OrderDate = ordenActualizada.OrderDate;
                ordenBD.StartDate = ordenActualizada.StartDate;
                ordenBD.EndDate = ordenActualizada.EndDate;
                ordenBD.StatusOrder = ordenActualizada.StatusOrder;
                ordenBD.DescuentoManual = ordenActualizada.DescuentoManual;

                // 4) Agregar los nuevos detalles **validando stock**
                foreach (var det in ordenActualizada.Details)
                {
                    var equipo = db.Equipment.FirstOrDefault(e => e.EquipmentId == det.EquipmentId);

                    if (equipo == null)
                    {
                        throw new InvalidOperationException("Uno de los equipos seleccionados no existe.");
                    }

                    // ✅ Validación de concurrencia: que aún haya stock suficiente
                    if (equipo.Stock < det.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"No hay suficiente stock para el equipo: {equipo.EquipmentName}. " +
                            $"Disponibles: {equipo.Stock}, seleccionados: {det.Quantity}."
                        );
                    }

                    // Si hay stock suficiente, agregamos el detalle
                    var nuevoDetalle = new OrderDetailDA
                    {
                        OrderId = ordenBD.OrderId,
                        EquipmentId = det.EquipmentId,
                        Quantity = det.Quantity
                    };
                    db.OrderDetails.Add(nuevoDetalle);

                    // y descontamos del stock
                    equipo.Stock -= det.Quantity;

                    if (equipo.Stock <= 0)
                    {
                        equipo.Stock = 0;
                        equipo.Status = 2; // 2 = Sin stock / No disponible
                    }
                }

                // 5) Guardar todos los cambios
                return db.SaveChanges();
            }
        }
    }
}
