using System;
using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Empleado.EditarEmpleado
{
    public class EditarEmpleadoAD : IEditarEmpleadoAD
    {
        private Contexto elContexto;

        public EditarEmpleadoAD()
        {
            elContexto = new Contexto();
        }

        public int Editar(EmpleadoDto elEmpleadoParaActualizar)
        {
            EmpleadoDA elEmpleadoEnBaseDeDatos = elContexto.Empleado
                .FirstOrDefault(e => e.IdEmpleado == elEmpleadoParaActualizar.IdEmpleado);

            if (elEmpleadoEnBaseDeDatos == null)
                return 0;

            elEmpleadoEnBaseDeDatos.Nombre = elEmpleadoParaActualizar.Nombre;
            elEmpleadoEnBaseDeDatos.Apellido = elEmpleadoParaActualizar.Apellido;
            elEmpleadoEnBaseDeDatos.TelefonoCelular = elEmpleadoParaActualizar.TelefonoCelular;
            elEmpleadoEnBaseDeDatos.CorreoElectronico = elEmpleadoParaActualizar.CorreoElectronico;
            elEmpleadoEnBaseDeDatos.RolId = elEmpleadoParaActualizar.RolId;
            elEmpleadoEnBaseDeDatos.Estado = elEmpleadoParaActualizar.Estado;
            elEmpleadoEnBaseDeDatos.IdEmpleadoGuid = elEmpleadoParaActualizar.IdEmpleadoGuid
                ?? (elEmpleadoEnBaseDeDatos.IdEmpleadoGuid ?? Guid.NewGuid());

            elEmpleadoEnBaseDeDatos.Cedula = elEmpleadoParaActualizar.Cedula;
            elEmpleadoEnBaseDeDatos.ContactoEmergenciaNombre = elEmpleadoParaActualizar.ContactoEmergenciaNombre;
            elEmpleadoEnBaseDeDatos.ContactoEmergenciaTelefono = elEmpleadoParaActualizar.ContactoEmergenciaTelefono;
            elEmpleadoEnBaseDeDatos.ContactoEmergenciaParentesco = elEmpleadoParaActualizar.ContactoEmergenciaParentesco;
            elEmpleadoEnBaseDeDatos.TipoSangre = elEmpleadoParaActualizar.TipoSangre;
            elEmpleadoEnBaseDeDatos.Alergias = elEmpleadoParaActualizar.Alergias;
            elEmpleadoEnBaseDeDatos.InfoMedica = elEmpleadoParaActualizar.InfoMedica;

            if (!string.IsNullOrWhiteSpace(elEmpleadoParaActualizar.FotoPerfil))
                elEmpleadoEnBaseDeDatos.FotoPerfil = elEmpleadoParaActualizar.FotoPerfil;

            return elContexto.SaveChanges();
        }
    }
}
