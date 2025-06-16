using SetLight.Abstracciones.ModelosParaUI;
using System.Collections.Generic;

namespace SetLight.Abstracciones.LogicaDeNegocio.RentalOrder.ListarRentalOrder
{
    public interface IListarRentalOrderLN
    {
        List<RentalOrderDto> Obtener();
    }
}
