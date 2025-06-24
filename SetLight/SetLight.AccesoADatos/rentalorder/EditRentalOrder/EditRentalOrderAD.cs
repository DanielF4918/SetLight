using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.EditRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.rentalorder.EditRentalOrder
{
    public class EditRentalOrderAD:IEditRentalOrderAD
    {
        private Contexto elContexto;

        public EditRentalOrderAD()
        {
            elContexto = new Contexto();
        }

        public int Editar(RentalOrderDto ROParaActualizar)
        {
            RentalOrderDA  ROEnBaseDeDatos = elContexto.RentalOrders.Where(RO => RO.OrderId == ROParaActualizar.OrderId).FirstOrDefault();
            ROEnBaseDeDatos.ClientId = ROParaActualizar.ClientId;
            ROEnBaseDeDatos.OrderDate = ROParaActualizar.OrderDate;
            ROEnBaseDeDatos.StartDate = ROParaActualizar.StartDate;
            ROEnBaseDeDatos.EndDate = ROParaActualizar.EndDate;
            ROEnBaseDeDatos.StatusOrder = ROParaActualizar.StatusOrder;
            
            int seGuardoRO = elContexto.SaveChanges();
            return seGuardoRO;

        }
    }
}
