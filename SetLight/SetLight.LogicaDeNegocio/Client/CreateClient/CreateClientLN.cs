using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            // Puedes aplicar reglas de negocio aquí si es necesario
            int id = await _crearClientAD.Guardar(clientAGuardar);
            return id;
        }
    }
}
