using SetLight.Abstracciones.AccesoADatos.RentalOrder.ListarRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;
using System.Collections.Generic;
using System.Linq;

namespace SetLight.AccesoADatos.RentalOrder
{
    public class ListarRentalOrderAD : IListarRentalOrder
    {
        public List<RentalOrderDto> Obtener()
        {
            using (var db = new Contexto())
            {
                var lista = (from orden in db.RentalOrders
                             join cliente in db.Clients on orden.ClientId equals cliente.ClientId
                             select new RentalOrderDto
                             {
                                 OrderId = orden.OrderId,
                                 ClientId = orden.ClientId,
                                 ClientName = cliente.FirstName + " " + cliente.LastName,
                                 OrderDate = orden.OrderDate,
                                 StartDate = orden.StartDate,
                                 EndDate = orden.EndDate,
                                 StatusOrder = orden.StatusOrder
                             }).ToList();

                return lista;
            }
        }
    }
}
