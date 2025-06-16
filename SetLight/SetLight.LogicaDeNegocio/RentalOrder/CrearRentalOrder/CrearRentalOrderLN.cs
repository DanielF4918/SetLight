using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.CrearRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;

public class CrearRentalOrderLN
{
    private readonly ICrearRentalOrderAD _accesoAD;

    public CrearRentalOrderLN(ICrearRentalOrderAD accesoAD)
    {
        _accesoAD = accesoAD;
    }

    public async Task<int> Guardar(RentalOrderDto orden)
    {

        return await _accesoAD.Guardar(orden);
    }
}
