using System.Collections.Generic;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.ListarRentalOrder;
using SetLight.Abstracciones.LogicaDeNegocio.RentalOrder.ListarRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.LogicaDeNegocio.RentalOrder
{
    public class ListarRentalOrderLN : IListarRentalOrderLN
    {
        private readonly IListarRentalOrder _listarAD;

        public ListarRentalOrderLN(IListarRentalOrder listarAD)
        {
            _listarAD = listarAD;
        }

        public List<RentalOrderDto> Obtener()
        {
            return _listarAD.Obtener();
        }
    }
}
