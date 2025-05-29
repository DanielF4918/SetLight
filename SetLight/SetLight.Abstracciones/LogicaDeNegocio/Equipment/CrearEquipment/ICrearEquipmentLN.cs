using SetLight.Abstracciones.ModelosParaUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.LogicaDeNegocio.Equipment.CrearEquipment
{
    public interface ICrearEquipmentLN
    {
        Task<int> Guardar(EquipmentDto equipmentaguardar);
    }
}
