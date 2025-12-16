using System.Collections.Generic;
using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Trazabilidad;
using SetLight.Abstracciones.LogicaDeNegocio.Trazabilidad;
using SetLight.Abstracciones.ModelosParaUI;
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

            var resultado = data
                .OrderBy(x => x.FechaInicio ?? x.FechaMantenimiento)
                .ToList();

            return resultado;
        }
    }
}
