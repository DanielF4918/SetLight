using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Empleado.EditarEmpleado
{
    public class EditarEmpleadoAD: IEditarEmpleadoAD
    {
        private Contexto elContexto;

        public EditarEmpleadoAD()
        {
            elContexto = new Contexto();
        }

        public int Editar(EmpleadoDto elEmpleadoParaActualizar)
        {
            EmpleadoDA elEmpleadoEnBaseDeDatos = elContexto.Empleado.Where(elEmpleado => elEmpleado.IdEmpleado == elEmpleadoParaActualizar.IdEmpleado).FirstOrDefault();

                elEmpleadoEnBaseDeDatos.Nombre = elEmpleadoParaActualizar.Nombre;
                elEmpleadoEnBaseDeDatos.Apellido = elEmpleadoParaActualizar.Apellido;
                elEmpleadoEnBaseDeDatos.TelefonoCelular = elEmpleadoParaActualizar.TelefonoCelular;
                elEmpleadoEnBaseDeDatos.CorreoElectronico = elEmpleadoParaActualizar.CorreoElectronico;
                elEmpleadoEnBaseDeDatos.RolId = elEmpleadoParaActualizar.RolId;
                elEmpleadoEnBaseDeDatos.Estado = elEmpleadoParaActualizar.Estado;
                int seGuardoElEmpleado = elContexto.SaveChanges();
                return seGuardoElEmpleado;
        }
            

    }
}
