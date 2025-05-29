using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Equipment.ObtenerEqPorID;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.AccesoADatos.Equipment.ObtenerEqPorID
{
    public class ObtenerEqPorIDAD:IObtenerEqPorIDAD
    {
        private Contexto _elContexto;

        public ObtenerEqPorIDAD()
        {
            _elContexto = new Contexto();
        }

        public EquipmentDto Obtener(int id)
        {
            EquipmentDto listaDelEquipmentARetornar = (from elEquipment in _elContexto.Equipment
                                                       where elEquipment.EquipmentId == id
                                                       select new EquipmentDto
                                                       {
                                                           EquipmentId = elEquipment.EquipmentId,
                                                           EquipmentName = elEquipment.EquipmentName,
                                                           Brand = elEquipment.Brand,
                                                           Model = elEquipment.Model,
                                                           SerialNumber = elEquipment.SerialNumber,
                                                           Description = elEquipment.Description,
                                                           Status = elEquipment.Status,
                                                           CategoryId = elEquipment.CategoryId


                                                       }).FirstOrDefault();
            return listaDelEquipmentARetornar;

        }

    }
}
