using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Client.ListClient;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.AccesoADatos.Client.ListClient
{
    public class ListarClientAD : IListarClientAD
    {
        private readonly Contexto _contexto;

        public ListarClientAD()
        {
            _contexto = new Contexto();
        }

        public List<ClientDto> Obtener()
        {
            var listaDeClientesARetornar = (
                from cliente in _contexto.Clients
                select new ClientDto
                {
                    ClientId = cliente.ClientId,
                    FirstName = cliente.FirstName,
                    LastName = cliente.LastName,
                    Phone = cliente.Phone,
                    Email = cliente.Email,
                    Status = cliente.Status
                }).ToList();

            return listaDeClientesARetornar;
        }
    }
}
