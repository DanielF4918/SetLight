using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.Abstracciones.LogicaDeNegocio.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Empleado.CrearEmpleado;

namespace SetLight.LogicaDeNegocio.Empleado.CrearEmpleado
{
    public class CrearEmpleadoLN : ICrearEmpleadoLN
    {
        private readonly ICrearEmpleadoAD _crearEmpleadoAD;
        public CrearEmpleadoLN()
        {
            _crearEmpleadoAD = new CrearEmpleadoAD();
        }
        public async Task<int> Guardar(EmpleadoDto empleadoAGuardar)
        {
            int id = await _crearEmpleadoAD.Guardar(empleadoAGuardar);
            return id;
        }
    }
}
