using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Equipment.ListarEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Equipment.ListarEquipment;

namespace SetLight.LogicaDeNegocio.Equipment.ListarEquipment
{
    public class ListarEquipmentLN: IListarEquipmentLN
    {
        private IListarEquipmentAD _listarEquipmentAD;
        public ListarEquipmentLN() {
            _listarEquipmentAD = new ListarEquipmentAD();

        }

        public List<EquipmentDto> Obtener()
        {
            List<EquipmentDto> LaListaEquipment = _listarEquipmentAD.Obtener();
            return LaListaEquipment;
        }

    }
}
