using System.Collections.Generic;
using System.Linq;
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
            var listaDeEquipmentARetornar = (
                from equipment in _contexto.Equipment
                join categoria in _contexto.EqCategory
                on equipment.CategoryId equals categoria.CategoryId
                select new EquipmentDto
                {
                    EquipmentId = equipment.EquipmentId,
                    EquipmentName = equipment.EquipmentName,
                    Brand = equipment.Brand,
                    Model = equipment.Model,
                    SerialNumber = equipment.SerialNumber,
                    Description = equipment.Description,
                    RentalValue = equipment.RentalValue,
                    CategoryId = equipment.CategoryId,
                    Status = equipment.Status,
                    CategoriaNombre = categoria.CategoryName
                }).ToList();

            return listaDeEquipmentARetornar;
        }
    }
}
