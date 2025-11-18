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
    public class EditClientAD : IEditClientAD
    {
        private Contexto ElContexto;

        public EditClientAD()
        {
            ElContexto = new Contexto();
        }

        public int Editar(ClientDto clientParaActualizar)
        {
            ClientDa clientEnBaseDeDatos = ElContexto.Clients
                .FirstOrDefault(client => client.ClientId == clientParaActualizar.ClientId);

            if (clientEnBaseDeDatos == null)
                return 0; 

            clientEnBaseDeDatos.FirstName = clientParaActualizar.FirstName;
            clientEnBaseDeDatos.LastName = clientParaActualizar.LastName;
            clientEnBaseDeDatos.Phone = clientParaActualizar.Phone;
            clientEnBaseDeDatos.Email = clientParaActualizar.Email;
            clientEnBaseDeDatos.Status = clientParaActualizar.Status;

            clientEnBaseDeDatos.EmpresaNombre = clientParaActualizar.EmpresaNombre;
            clientEnBaseDeDatos.EmpresaTelefono = clientParaActualizar.EmpresaTelefono;

            int seGuardo = ElContexto.SaveChanges();
            return seGuardo;
        }
    }
}
