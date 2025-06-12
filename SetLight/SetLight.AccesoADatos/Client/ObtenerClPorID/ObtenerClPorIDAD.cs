using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Client.ObtenerClPorId;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.AccesoADatos.Clientes.ObtenerClPorID
{
    public class ObtenerClPorIDAD : IObtenerClPorIDAD
    {
        private Contexto _elContexto;

        public ObtenerClPorIDAD()
        {
            _elContexto = new Contexto();
        }

        public ClientDto Obtener(int id)
        {
            ClientDto clienteARetornar = (from c in _elContexto.Clients
                                          where c.ClientId == id
                                          select new ClientDto
                                          {
                                              ClientId = c.ClientId,
                                              FirstName = c.FirstName,
                                              LastName = c.LastName,
                                              Phone = c.Phone,
                                              Email = c.Email
                                          }).FirstOrDefault();

            return clienteARetornar;
        }
    }
}
