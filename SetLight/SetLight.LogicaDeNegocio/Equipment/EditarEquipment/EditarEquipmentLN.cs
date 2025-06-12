using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Equipment.EditarEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.EditarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Equipment.EditarEquipment;

namespace SetLight.LogicaDeNegocio.Equipment.EditarEquipment
{
    public class EditarEquipmentLN : IEquipmentLN

    {
        private IEditarEquipmentAD _actualizarEquipment;

        public EditarEquipmentLN()
        {
            _actualizarEquipment = new EditarEquipmentAD();
        }

        public int Actualizar(EquipmentDto elEquipmentParaActualizar)
        {
            return _actualizarEquipment.Editar(elEquipmentParaActualizar);
        }
    }
}
