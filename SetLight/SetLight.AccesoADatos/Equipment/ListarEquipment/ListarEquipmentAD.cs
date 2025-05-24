using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Equipment.ListarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Equipment.ListarEquipment
{
    public class ListarEquipmentAD : IListarEquipmentAD
    {
        private readonly Contexto _contexto;

        public ListarEquipmentAD()
        {
            _contexto = new Contexto();
        }

        public List<EquipmentDto> Obtener()
        {
            List<EquipmentDA> listaDeEquipment = _contexto.Equipment.ToList();

            List<EquipmentDto> listaDeEquipmentARetornar = (from equipment in _contexto.Equipment
                                                          select new EquipmentDto
                                                          {
                                                              EquipmentId = equipment.EquipmentId,
                                                              EquipmentName =equipment.EquipmentName,
                                                              Brand = equipment.Brand,
                                                              Model = equipment.Model,
                                                              SerialNumber = equipment.SerialNumber,
                                                              Description = equipment.Description,
                                                              CategoryId = equipment.CategoryId,
                                                              Status = equipment.Status
                                                          }).ToList();

            return listaDeEquipmentARetornar;
        }
    }
}