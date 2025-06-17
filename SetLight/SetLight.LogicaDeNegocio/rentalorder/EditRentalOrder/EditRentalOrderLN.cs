using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.EditRentalOrder;
using SetLight.Abstracciones.LogicaDeNegocio.rentalorder.EditRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.rentalorder.EditRentalOrder;

namespace SetLight.LogicaDeNegocio.rentalorder.EditRentalOrder
{
    public class EditRentalOrderLN:IEditRentalOrderLN
    {
        private IEditRentalOrderAD _actualizarRentalOrder;

        public EditRentalOrderLN()
        {
            _actualizarRentalOrder = new EditRentalOrderAD();
        }

        public int Actualizar(RentalOrderDto rentalOrderParaActualizar)
        {

            return _actualizarRentalOrder.Editar(rentalOrderParaActualizar);
        }
    }
}
