using System.Collections.Generic;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.LogicaDeNegocio.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Empleado.ListarEmpleado;

namespace SetLight.LogicaDeNegocio.Empleado.ListarEmpleado
{
    public class ListarEmpleadoLN : IListarEmpleadoLN
    {
        private IListarEmpleadoAD _listarEmpleadoAD;

        public ListarEmpleadoLN()
        {
            _listarEmpleadoAD = new ListarEmpleadoAD();
        }

        public List<EmpleadoDto> Obtener()
        {
            List<EmpleadoDto> listaARetornar = _listarEmpleadoAD.Obtener();
            return listaARetornar;
        }
    }
}
