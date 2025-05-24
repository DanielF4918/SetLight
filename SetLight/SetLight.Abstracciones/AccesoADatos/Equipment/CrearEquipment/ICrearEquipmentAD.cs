using SetLight.Abstracciones.ModelosParaUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.AccesoADatos.Equipment.CrearEquipment
{
    public interface ICrearEquipmentAD
    {

        Task<int> Guardar(EquipmentDto equipmentaguardar);
    }
}
