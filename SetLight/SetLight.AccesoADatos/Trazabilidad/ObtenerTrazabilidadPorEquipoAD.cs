using System;
using System.Collections.Generic;
using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Trazabilidad;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;
using SetLight.Entidades;

namespace SetLight.AccesoADatos.Trazabilidad
{
    public class ObtenerTrazabilidadPorEquipoAD : ITrazabilidadAD
    {
        public List<TrazabilidadDto> ObtenerPorEquipo(int equipoId)
        {
            using (var contexto = new Contexto())
            {
                var prestamos = from orden in contexto.RentalOrders
                                join cliente in contexto.Clients on orden.ClientId equals cliente.ClientId
                                join detalle in contexto.OrderDetails on orden.OrderId equals detalle.OrderId
                                join equipo in contexto.Equipment on detalle.EquipmentId equals equipo.EquipmentId
                                where detalle.EquipmentId == equipoId
                                select new TrazabilidadDto
                                {
                                    EquipmentId = equipo.EquipmentId,
                                    EquipmentNombre = equipo.EquipmentName,
                                    TipoEvento = "Préstamo",
                                    ClienteNombre = cliente.FirstName + " " + cliente.LastName,
                                    FechaInicio = orden.StartDate,
                                    FechaFin = orden.EndDate,
                                    EncargadoPrestamo = "", // Puedes ajustarlo si tenés campo
                                    FechaMantenimiento = null,
                                    TipoMantenimiento = 1,
                                    Comentarios = null
                                };

                var mantenimientos = from m in contexto.Maintenance
                                     join equipo in contexto.Equipment on m.EquipmentId equals equipo.EquipmentId
                                     where m.EquipmentId == equipoId
                                     select new TrazabilidadDto
                                     {
                                         EquipmentId = equipo.EquipmentId,
                                         EquipmentNombre = equipo.EquipmentName,
                                         TipoEvento = "Mantenimiento",
                                         ClienteNombre = null,
                                         FechaInicio = null,
                                         FechaFin = m.EndDate,
                                         EncargadoPrestamo = null,
                                         FechaMantenimiento = m.StartDate,
                                         TipoMantenimiento = m.MaintenanceType,
                                         Comentarios = "Mantenimiento programado"
                                     };

                return prestamos.Concat(mantenimientos).ToList();

            }
        }
    }
}
