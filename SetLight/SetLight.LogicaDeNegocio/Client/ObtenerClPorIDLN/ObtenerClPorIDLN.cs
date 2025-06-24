using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Client.ObtenerClPorId;
using SetLight.Abstracciones.LogicaDeNegocio.Client.ObtenerClPorId;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Clientes.ObtenerClPorID;

namespace SetLight.LogicaDeNegocio.Client.ObtenerClPorIDLN
{
    public class ObtenerClPorIDLN : IObtenerClPorIDLN
    {
        private IObtenerClPorIDAD _obtenerClPorIDAD;

        public ObtenerClPorIDLN()
        {
            _obtenerClPorIDAD = new ObtenerClPorIDAD();
        }

        public ClientDto Obtener(int id)
        {
            ClientDto client = _obtenerClPorIDAD.Obtener(id);
            return client;
        }
    }
}
