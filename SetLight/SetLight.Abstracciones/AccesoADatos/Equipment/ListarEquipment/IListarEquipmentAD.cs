using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.Abstracciones.AccesoADatos.Equipment.ListarEquipment
{
    public interface IListarEquipmentAD
    {
        List<EquipmentDto> Obtener();
    }
}
