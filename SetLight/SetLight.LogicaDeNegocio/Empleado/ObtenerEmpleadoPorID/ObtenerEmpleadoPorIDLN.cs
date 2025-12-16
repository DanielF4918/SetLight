using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.LogicaDeNegocio.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Empleado.ObtenerEmpleadoPorID;

namespace SetLight.LogicaDeNegocio.Empleado.ObtenerEmpleadoPorID
{
    public class ObtenerEmpleadoPorIDLN : IObtenerEmpleadoPorIDLN
    {
        private IObtenerEmpleadoPorIDAD _obtenerEmpleadoPorIDAD;

        public ObtenerEmpleadoPorIDLN()
        {
            _obtenerEmpleadoPorIDAD = new ObtenerEmpleadoPorIDAD();
        }

        public EmpleadoDto Obtener(int id)
        {
            EmpleadoDto elEmpleado = _obtenerEmpleadoPorIDAD.Obtener(id);
            return elEmpleado;
        }
    }
}
