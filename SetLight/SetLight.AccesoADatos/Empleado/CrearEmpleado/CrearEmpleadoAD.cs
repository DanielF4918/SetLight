using System;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Empleado.CrearEmpleado
{
    public class CrearEmpleadoAD : ICrearEmpleadoAD
    {
        private Contexto elContexto;

        public CrearEmpleadoAD()
        {
            elContexto = new Contexto();
        }

        public async Task<int> Guardar(EmpleadoDto empleadoAGuardar)
        {
            elContexto.Empleado.Add(ConvierteEmpleado(empleadoAGuardar));
            int resultado = await elContexto.SaveChangesAsync();
            return resultado;
        }

        private EmpleadoDA ConvierteEmpleado(EmpleadoDto empleado)
        {
            return new EmpleadoDA
            {
                IdEmpleadoGuid = empleado.IdEmpleadoGuid ?? Guid.NewGuid(),
                Nombre = empleado.Nombre,
                Apellido = empleado.Apellido,
                TelefonoCelular = empleado.TelefonoCelular,
                CorreoElectronico = empleado.CorreoElectronico,
                RolId = empleado.RolId,
                Estado = empleado.Estado,
                Cedula = empleado.Cedula,
                ContactoEmergenciaNombre = empleado.ContactoEmergenciaNombre,
                ContactoEmergenciaTelefono = empleado.ContactoEmergenciaTelefono,
                ContactoEmergenciaParentesco = empleado.ContactoEmergenciaParentesco,
                TipoSangre = empleado.TipoSangre,
                Alergias = empleado.Alergias,
                InfoMedica = empleado.InfoMedica,

                FotoPerfil = empleado.FotoPerfil
            };
        }
    }
}
