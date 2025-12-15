using SetLight.Abstracciones.AccesoADatos.Client.EditClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.EditClient;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Client.EditClient;

namespace SetLight.LogicaDeNegocio.Client.EditClient
{
    public class EditClientLN : IEditClientLN
    {
        private IEditClientAD _actualizarClient;

        public EditClientLN()
        {
            _actualizarClient = new EditClientAD();
        }

        public int Actualizar(ClientDto clientParaActualizar)
        {
            return _actualizarClient.Editar(clientParaActualizar);

        }
    }
}
