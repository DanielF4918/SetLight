using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.ObtenerROPorID;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.AccesoADatos.rentalorder.ObtenerROPorId
{
    public class ObtenerROPorIdAD: IObtenerROPorIdAD
    {
        private Contexto _elContexto;

        public ObtenerROPorIdAD()
        {
            _elContexto = new Contexto();
        }

        public RentalOrderDto Obtener(int id)
        {
            RentalOrderDto ROaRetornar = (from RO in _elContexto.RentalOrders
                                          where RO.OrderId == id
                                          select new RentalOrderDto
                                          {
                                              OrderId = RO.OrderId,
                                              ClientId = RO.ClientId,
                                              OrderDate = RO.OrderDate,
                                              StartDate = RO.StartDate,
                                              EndDate = RO.EndDate,
                                              StatusOrder = RO.StatusOrder

                                          }).FirstOrDefault();
            return ROaRetornar;
        }
        
    }
}
