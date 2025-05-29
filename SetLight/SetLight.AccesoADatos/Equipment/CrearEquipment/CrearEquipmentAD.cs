using SetLight.Abstracciones.AccesoADatos.Equipment.CrearEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.AccesoADatos.Equipment.CrearEquipment
{
    public class CrearEquipmentAD : ICrearEquipmentAD
    {
        private Contexto elContexto;

        public CrearEquipmentAD()
        {
            elContexto = new Contexto();
        }
        public async Task<int> Guardar(EquipmentDto equipmentaguardar)
        {
            elContexto.Equipment.Add(ConvierteEquipment(equipmentaguardar));
            
            int resultado = await elContexto.SaveChangesAsync();
            return resultado;
        }

        private EquipmentDA ConvierteEquipment(EquipmentDto equipment)
        {
            return new EquipmentDA
            {
                EquipmentId = equipment.EquipmentId,
                EquipmentName = equipment.EquipmentName,
                Brand = equipment.Brand,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                Description = equipment.Description,
                CategoryId = equipment.CategoryId,
                Status = equipment.Status
            };
        }
    }
}
