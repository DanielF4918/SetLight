using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Equipment.ObtenerEqPorID;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Equipment.ObtenerEqPorID;

namespace SetLight.LogicaDeNegocio.Equipment.ObtenerEqPorID
{
    public class ObtenerEqPorIDLN:IObtenerEqPorIDLN
    {
        private IObtenerEqPorIDAD _obtenerEqPorIDAD;

        public ObtenerEqPorIDLN()
        {
            _obtenerEqPorIDAD = new ObtenerEqPorIDAD();

        }

        public EquipmentDto Obtener(int id)
        {
           EquipmentDto elEquipment = _obtenerEqPorIDAD.Obtener(id);
            return elEquipment;
        }
    }
}
