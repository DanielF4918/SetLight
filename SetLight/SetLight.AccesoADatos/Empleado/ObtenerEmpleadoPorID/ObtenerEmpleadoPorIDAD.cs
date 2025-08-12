using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.AccesoADatos.Empleado.ObtenerEmpleadoPorID
{
    public class ObtenerEmpleadoPorIDAD: IObtenerEmpleadoPorIDAD
    {
        private Contexto _elContexto;
        public ObtenerEmpleadoPorIDAD()
        {
            _elContexto = new Contexto();
        }
        public EmpleadoDto Obtener(int id)
        {
            EmpleadoDto empleadoARetornar = (from elEmpleado in _elContexto.Empleado
                                             where elEmpleado.IdEmpleado == id
                                             select new EmpleadoDto
                                             {
                                                 IdEmpleado = elEmpleado.IdEmpleado,
                                                 Nombre = elEmpleado.Nombre,
                                                 Apellido = elEmpleado.Apellido,
                                                 TelefonoCelular = elEmpleado.TelefonoCelular,
                                                 CorreoElectronico = elEmpleado.CorreoElectronico,
                                                 RolId = elEmpleado.RolId,
                                                 Estado = elEmpleado.Estado
                                             }).FirstOrDefault();
            return empleadoARetornar;
        }
    }
}
