using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Client.EditClient;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Client.EditClient
{
    public class EditClientAD:IEditClientAD
    {
        private Contexto ElContexto;

        public EditClientAD()
        {
            ElContexto = new Contexto();
        }

        public int Editar(ClientDto clientParaActualizar)
        {
            ClientDa clientEnBaseDeDatos = ElContexto.Clients.Where(client => client.ClientId == clientParaActualizar.ClientId).FirstOrDefault();
            clientEnBaseDeDatos.FirstName = clientParaActualizar.FirstName;
            clientEnBaseDeDatos.LastName = clientParaActualizar.LastName;
            clientEnBaseDeDatos.Phone = clientParaActualizar.Phone;
            clientEnBaseDeDatos.Email = clientParaActualizar.Email;
            clientEnBaseDeDatos.Status = clientParaActualizar.Status;

            int seGuardo = ElContexto.SaveChanges();
            return seGuardo;

        }
    }
}
