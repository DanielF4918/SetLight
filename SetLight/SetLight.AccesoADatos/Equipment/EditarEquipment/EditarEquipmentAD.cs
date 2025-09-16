using System.Linq;
using SetLight.Abstracciones.AccesoADatos.Equipment.EditarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Equipment.EditarEquipment
{
    public class EditarEquipmentAD : IEditarEquipmentAD
    {
        private readonly Contexto elContexto;

        public EditarEquipmentAD()
        {
            elContexto = new Contexto();
        }

        public int Editar(EquipmentDto elEquipmentParaActualizar)
        {
            var entidad = elContexto.Equipment
                .FirstOrDefault(e => e.EquipmentId == elEquipmentParaActualizar.EquipmentId);

            if (entidad == null)
                return 0;

            entidad.EquipmentName = elEquipmentParaActualizar.EquipmentName;
            entidad.Brand = elEquipmentParaActualizar.Brand;
            entidad.Model = elEquipmentParaActualizar.Model;
            entidad.SerialNumber = elEquipmentParaActualizar.SerialNumber;
            entidad.Description = elEquipmentParaActualizar.Description;
            entidad.RentalValue = elEquipmentParaActualizar.RentalValue;
            entidad.Stock = elEquipmentParaActualizar.Stock;
            entidad.CategoryId = elEquipmentParaActualizar.CategoryId;
            entidad.Status = elEquipmentParaActualizar.Status;


            entidad.ImageUrl = string.IsNullOrWhiteSpace(elEquipmentParaActualizar.ImageUrl)
                ? null
                : elEquipmentParaActualizar.ImageUrl;

            return elContexto.SaveChanges();
        }
    }
}
