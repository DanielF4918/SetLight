using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Equipment.ObtenerEqPorID;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.AccesoADatos.Equipment.ObtenerEqPorID
{
    public class ObtenerEqPorIDAD : IObtenerEqPorIDAD
    {
        private readonly Contexto _elContexto;

        public ObtenerEqPorIDAD()
        {
            _elContexto = new Contexto();
        }

        public EquipmentDto Obtener(int id)
        {
            var equipmentARetornar =
                (from e in _elContexto.Equipment
                 join c in _elContexto.EqCategory on e.CategoryId equals c.CategoryId into cj
                 from c in cj.DefaultIfEmpty() // LEFT JOIN
                 where e.EquipmentId == id
                 select new EquipmentDto
                 {
                     EquipmentId = e.EquipmentId,
                     EquipmentName = e.EquipmentName,
                     Brand = e.Brand,
                     Model = e.Model,
                     SerialNumber = e.SerialNumber,
                     Description = e.Description,
                     RentalValue = e.RentalValue,
                     Stock = e.Stock,
                     Status = e.Status,
                     CategoryId = e.CategoryId,
                     CategoriaNombre = c != null ? c.CategoryName : null,
                     ImageUrl = e.ImageUrl
                 })
                .FirstOrDefault();

            return equipmentARetornar;
        }
    }
}
