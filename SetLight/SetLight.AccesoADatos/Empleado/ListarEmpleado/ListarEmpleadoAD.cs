using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Empleado;

namespace SetLight.AccesoADatos.Empleado.ListarEmpleado
{
    public class ListarEmpleadoAD : IListarEmpleadoAD
    {
        private Contexto _elContexto;

        public ListarEmpleadoAD()
        {
            _elContexto = new Contexto();
        }

        public List<EmpleadoDto> Obtener()
        {
            List<EmpleadoDA> listaEmpleado = _elContexto.Empleado.ToList();
            List<EmpleadoDto> listaARetornar = (from empleado in _elContexto.Empleado
                                                select new EmpleadoDto
                                                {
                                                    IdEmpleado = empleado.IdEmpleado,
                                                    IdEmpleadoGuid = empleado.IdEmpleadoGuid,
                                                    Nombre = empleado.Nombre,
                                                    Apellido = empleado.Apellido,
                                                    TelefonoCelular = empleado.TelefonoCelular,
                                                    CorreoElectronico = empleado.CorreoElectronico,
                                                    RolId = empleado.RolId,
                                                    Estado = empleado.Estado
                                                }).ToList();

            return listaARetornar;
        }
    }
}
