using System;
using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.AccesoADatos.Empleado.ObtenerEmpleadoPorID
{
    public class ObtenerEmpleadoPorIDAD : IObtenerEmpleadoPorIDAD
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
                                                 IdEmpleadoGuid = elEmpleado.IdEmpleadoGuid,
                                                 Nombre = elEmpleado.Nombre,
                                                 Apellido = elEmpleado.Apellido,
                                                 TelefonoCelular = elEmpleado.TelefonoCelular,
                                                 CorreoElectronico = elEmpleado.CorreoElectronico,
                                                 RolId = elEmpleado.RolId,
                                                 Estado = elEmpleado.Estado,
                                                 Cedula = elEmpleado.Cedula,
                                                 ContactoEmergenciaNombre = elEmpleado.ContactoEmergenciaNombre,
                                                 ContactoEmergenciaTelefono = elEmpleado.ContactoEmergenciaTelefono,
                                                 ContactoEmergenciaParentesco = elEmpleado.ContactoEmergenciaParentesco,
                                                 TipoSangre = elEmpleado.TipoSangre,
                                                 Alergias = elEmpleado.Alergias,
                                                 InfoMedica = elEmpleado.InfoMedica,

                                                 FotoPerfil = elEmpleado.FotoPerfil
                                             }).FirstOrDefault();

            return empleadoARetornar;
        }
    }
}
