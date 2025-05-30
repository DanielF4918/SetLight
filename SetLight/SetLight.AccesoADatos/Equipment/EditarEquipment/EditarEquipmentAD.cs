using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.Equipment.EditarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.Equipment.EditarEquipment
{
    public class EditarEquipmentAD : IEditarEquipmentAD
    {
        private Contexto elContexto;
        public EditarEquipmentAD()
        {
            elContexto = new Contexto();
        }
        public int Editar(EquipmentDto elEquipmentParaActualizar)
        {
            EquipmentDA elEquipmentEnBaseDeDatos = elContexto.Equipment.Where(elEquipment => elEquipment.EquipmentId == elEquipmentParaActualizar.EquipmentId).FirstOrDefault();
            elEquipmentEnBaseDeDatos.EquipmentName = elEquipmentParaActualizar.EquipmentName;
            elEquipmentEnBaseDeDatos.Brand = elEquipmentParaActualizar.Brand;
            elEquipmentEnBaseDeDatos.Model = elEquipmentParaActualizar.Model;
            elEquipmentEnBaseDeDatos.SerialNumber = elEquipmentParaActualizar.SerialNumber;
            elEquipmentEnBaseDeDatos.Description = elEquipmentParaActualizar.Description;
            elEquipmentEnBaseDeDatos.CategoryId = elEquipmentParaActualizar.CategoryId;

            elEquipmentEnBaseDeDatos.Status = elEquipmentParaActualizar.Status;
            int seGuardoElEquipment = elContexto.SaveChanges();
            return seGuardoElEquipment;
        }
    }
}
