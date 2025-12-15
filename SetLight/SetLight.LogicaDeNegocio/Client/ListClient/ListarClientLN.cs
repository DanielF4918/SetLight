using System.Collections.Generic;
using System.Linq;
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
        public List<ClientDto> ObtenerActivos()
        {
            return Obtener().Where(c => c.Status == 1).ToList();
        }

    }
}
