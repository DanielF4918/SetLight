using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Client.CreateClient;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Client.CreateClient
{
    public class CrearClientAD : ICrearClientAD
    {
        private Contexto elContexto;

        public CrearClientAD()
        {
            elContexto = new Contexto();
        }

        public async Task<int> Guardar(ClientDto clientAGuardar)
        {
            elContexto.Clients.Add(ConvierteClient(clientAGuardar));
            int resultado = await elContexto.SaveChangesAsync();
            return resultado;
        }

        private ClientDa ConvierteClient(ClientDto client)
        {
            return new ClientDa
            {
                ClientId = client.ClientId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Phone = client.Phone,
                Email = client.Email
            };
        }
    }
}
