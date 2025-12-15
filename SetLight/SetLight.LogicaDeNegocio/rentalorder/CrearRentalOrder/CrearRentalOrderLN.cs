using System;
using System.Linq;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.CrearRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;

public class CrearRentalOrderLN
{
    private readonly ICrearRentalOrderAD _accesoAD;

    public CrearRentalOrderLN(ICrearRentalOrderAD accesoAD)
    {
        _accesoAD = accesoAD;
    }

    public async Task<int> Guardar(RentalOrderDto orden)
    {
        if (orden == null)
            throw new InvalidOperationException("La orden no puede ser nula.");

        // (Opcional) Validaciones rápidas de fechas
        if (orden.StartDate > orden.EndDate)
            throw new InvalidOperationException("La fecha de inicio no puede ser mayor que la fecha de fin.");

        // Validación crítica: EmpleadoId debe venir
        if (!orden.EmpleadoId.HasValue)
            throw new InvalidOperationException("No se puede finalizar: no se encontró un empleado activo asociado al usuario autenticado.");

        using (var db = new Contexto())
        {
            // ✅ Cliente debe existir y estar activo al momento de guardar
            var cliente = db.Clients.FirstOrDefault(c => c.ClientId == orden.ClientId);
            if (cliente == null)
                throw new InvalidOperationException("No se puede finalizar: el cliente no existe.");

            if (cliente.Status != 1)
                throw new InvalidOperationException("No se puede finalizar: el cliente está inactivo o fue desactivado.");

            // ✅ Empleado debe existir y estar activo al momento de guardar
            var empleado = db.Empleado.FirstOrDefault(e => e.IdEmpleado == orden.EmpleadoId.Value);
            if (empleado == null)
                throw new InvalidOperationException("No se puede finalizar: el empleado no existe.");

            if (!empleado.Estado)
                throw new InvalidOperationException("No se puede finalizar: su usuario fue desactivado mientras realizaba la orden.");
        }

        // Si todo OK, persistimos
        return await _accesoAD.Guardar(orden);
    }
}
