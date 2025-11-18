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
                // ========= PRÉSTAMOS =========
                var prestamosQuery =
                    from orden in contexto.RentalOrders
                    join cliente in contexto.Clients
                        on orden.ClientId equals cliente.ClientId
                    join detalle in contexto.OrderDetails
                        on orden.OrderId equals detalle.OrderId
                    join equipo in contexto.Equipment
                        on detalle.EquipmentId equals equipo.EquipmentId
                    join empleado in contexto.Empleado
                        on orden.EmpleadoId equals empleado.IdEmpleado into empleadoJoin
                    from empleado in empleadoJoin.DefaultIfEmpty()
                    where detalle.EquipmentId == equipoId
                    select new TrazabilidadDto
                    {
                        EquipmentId = equipo.EquipmentId,
                        EquipmentNombre = equipo.EquipmentName,
                        TipoEvento = "Préstamo",

                        OrderId = orden.OrderId,
                        MaintenanceId = null,

                        ClienteNombre = cliente.FirstName + " " + cliente.LastName,
                        FechaInicio = orden.StartDate,
                        FechaFin = orden.EndDate,

                        EncargadoPrestamo = empleado != null
                            ? (empleado.Nombre + " " + empleado.Apellido)
                            : "Colaborador no definido",

                        FechaMantenimiento = null,
                        TipoMantenimiento = 0,
                        Tecnico = null,
                        Comentarios = null
                    };

                // ========= MANTENIMIENTOS =========
                var mantenimientosQuery =
                    from m in contexto.Maintenance
                    join equipo in contexto.Equipment
                        on m.EquipmentId equals equipo.EquipmentId
                    // LEFT JOIN con Empleado del mantenimiento (técnico)
                    join tecnico in contexto.Empleado
                        on m.IdEmpleado equals tecnico.IdEmpleado into tecnicoJoin
                    from tecnico in tecnicoJoin.DefaultIfEmpty()
                    where m.EquipmentId == equipoId
                    select new TrazabilidadDto
                    {
                        EquipmentId = equipo.EquipmentId,
                        EquipmentNombre = equipo.EquipmentName,
                        TipoEvento = "Mantenimiento",

                        OrderId = null,
                        MaintenanceId = m.MaintenanceId,

                        ClienteNombre = null,
                        FechaInicio = null,
                        FechaFin = m.EndDate,
                        FechaMantenimiento = m.StartDate,

                        EncargadoPrestamo = null,
                        TipoMantenimiento = m.MaintenanceType,

                        // 👈 Aquí ya usamos el técnico real
                        Tecnico = tecnico != null
                            ? (tecnico.Nombre + " " + tecnico.Apellido)
                            : "—",

                        Comentarios = m.Comments ?? "Mantenimiento programado"
                    };

                var prestamos = prestamosQuery.ToList();
                var mantenimientos = mantenimientosQuery.ToList();

                return prestamos
                    .Concat(mantenimientos)
                    .ToList();
            }
        }
    }
}
