using SetLight.Abstracciones.AccesoADatos.Equipment.CrearEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.CrearEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Equipment.CrearEquipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.LogicaDeNegocio.Equipment.CrearEquipment
{
    public class CrearEquipmentLN : ICrearEquipmentLN
    {
        private ICrearEquipmentAD _createEquipmentAD;

        public CrearEquipmentLN()
        {
            _createEquipmentAD = new CrearEquipmentAD();
        }

        public async Task<int> Guardar(EquipmentDto equipmentguardar)
        {
            equipmentguardar.Status = 1;
            int id = await _createEquipmentAD.Guardar(equipmentguardar);
            return id;

        }
    }
}
