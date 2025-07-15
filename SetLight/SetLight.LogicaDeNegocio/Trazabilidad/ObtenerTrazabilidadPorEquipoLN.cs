using System;
using System.Collections.Generic;
using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Trazabilidad;
using SetLight.Abstracciones.LogicaDeNegocio.Trazabilidad;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Trazabilidad;

namespace SetLight.LogicaDeNegocio.Trazabilidad.ObtenerTrazabilidadPorEquipo
{
    public class ObtenerTrazabilidadPorEquipoLN : ITrazabilidadLN
    {
        private readonly ITrazabilidadAD _trazabilidadAD;

        public ObtenerTrazabilidadPorEquipoLN()
        {
            _trazabilidadAD = new ObtenerTrazabilidadPorEquipoAD();
        }

        public List<TrazabilidadDto> Ejecutar(int equipoId)
        {
            var data = _trazabilidadAD.ObtenerPorEquipo(equipoId);

            var resultado = data.Select(x => new TrazabilidadDto
            {
                EquipmentId = x.EquipmentId,
                EquipmentNombre = x.EquipmentNombre,
                TipoEvento = x.TipoEvento,

                // Para préstamos
                ClienteNombre = x.ClienteNombre,
                FechaInicio = x.FechaInicio,
                FechaFin = x.FechaFin,
                EncargadoPrestamo = x.EncargadoPrestamo,

                // Para mantenimientos
                FechaMantenimiento = x.FechaMantenimiento,
                TipoMantenimiento = x.TipoMantenimiento,
                Comentarios = x.Comentarios
            })
            .OrderBy(x => x.FechaInicio ?? x.FechaMantenimiento)
            .ToList();

            return resultado;
        }
    }
}
