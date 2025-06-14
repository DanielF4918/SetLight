using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.ObtenerROPorID;
using SetLight.Abstracciones.LogicaDeNegocio.rentalorder.ObtenerROPorID;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.rentalorder.ObtenerROPorId;

namespace SetLight.LogicaDeNegocio.rentalorder.ObtenerROPorId
{
    public class ObtenerROPorIdLN : IObtenerROPorIdLN
    {
        private IObtenerROPorIdAD _obtenerROPorIdAD;

        public ObtenerROPorIdLN()
        {
            _obtenerROPorIdAD = new ObtenerROPorIdAD();
        }

        public RentalOrderDto Obtener(int id)
        {
            RentalOrderDto rentalOrder = _obtenerROPorIdAD.Obtener(id);

            return _obtenerROPorIdAD.Obtener(id);
        }
    }
}
