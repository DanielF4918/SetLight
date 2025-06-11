using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Client.ListClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.ListClient;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Client.ListClient;



namespace SetLight.LogicaDeNegocio.Client.ListClient
{
    public class ListarClientLN : IListarClientLN
    {
        private readonly IListarClientAD _listarClientAD;

        public ListarClientLN()
        {
            _listarClientAD = new ListarClientAD();
        }

        public List<ClientDto> Obtener()
        {
            List<ClientDto> listaClientes = _listarClientAD.Obtener();
            return listaClientes;
        }
    }
}
