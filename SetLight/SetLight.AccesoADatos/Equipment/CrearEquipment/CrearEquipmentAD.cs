using SetLight.Abstracciones.AccesoADatos.Equipment.CrearEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;
using System.Threading.Tasks;

namespace SetLight.AccesoADatos.Equipment.CrearEquipment
{
    public class CrearEquipmentAD : ICrearEquipmentAD
    {
        private readonly Contexto elContexto;

        public CrearEquipmentAD()
        {
            elContexto = new Contexto();
        }

        public async Task<int> Guardar(EquipmentDto equipmentAGuardar)
        {
            var entidad = ConvierteEquipment(equipmentAGuardar);
            elContexto.Equipment.Add(entidad);

            int resultado = await elContexto.SaveChangesAsync();
            return resultado; 
        }

        private EquipmentDA ConvierteEquipment(EquipmentDto equipment)
        {
            return new EquipmentDA
            {
                EquipmentName = equipment.EquipmentName,
                Brand = equipment.Brand,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                Description = equipment.Description,
                RentalValue = equipment.RentalValue,
                Stock = equipment.Stock,
                CategoryId = equipment.CategoryId,
                Status = equipment.Status,

                ImageUrl = equipment.ImageUrl
            };
        }
    }
}
