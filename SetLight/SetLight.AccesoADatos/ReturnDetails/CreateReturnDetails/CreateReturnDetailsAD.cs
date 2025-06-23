using System;
using System.Linq;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.ReturnDetails.CreateReturnDetails;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.ReturnDetails.CreateReturnDetails
{
    public class CreateReturnDetailsAD : ICreateReturnDetailsAD
    {
        private Contexto elContexto;

        public CreateReturnDetailsAD()
        {
            elContexto = new Contexto();
        }

        public async Task<int> Guardar(ReturnDetailsDto returnDetailsAGuardar)
        {
            ReturnDetailDa nuevoDetalle = new ReturnDetailDa
            {
                OrderId = returnDetailsAGuardar.OrderId,
                EquipmentId = returnDetailsAGuardar.EquipmentId,
                ReturnDate = returnDetailsAGuardar.ReturnDate,
                ConditionReport = returnDetailsAGuardar.ConditionReport,
                IsReturned = returnDetailsAGuardar.IsReturned,
                RequiresMaintenance = returnDetailsAGuardar.RequiresMaintenance
            };

            elContexto.ReturnDetails.Add(nuevoDetalle);
            int seGuardo = await elContexto.SaveChangesAsync();

            return seGuardo;
        }
    }
}