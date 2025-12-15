using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Client.CreateClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.CreateClient;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Client.CreateClient;

namespace SetLight.LogicaDeNegocio.Client.CreateClient
{
    public class CrearClientLN : ICrearClientLN
    {
        private readonly ICrearClientAD _crearClientAD;

        public CrearClientLN()
        {
            _crearClientAD = new CrearClientAD();
        }

        public async Task<int> Guardar(ClientDto clientAGuardar)
        {
            int id = await _crearClientAD.Guardar(clientAGuardar);
            return id;
        }
    }
}
