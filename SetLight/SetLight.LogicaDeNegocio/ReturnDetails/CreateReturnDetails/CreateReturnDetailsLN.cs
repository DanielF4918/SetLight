using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.ReturnDetails.CreateReturnDetails;
using SetLight.Abstracciones.LogicaDeNegocio.ReturnDetails.CreateReturnDetails;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.LogicaDeNegocio.ReturnDetails.CreateReturnDetails
{
    public class CreateReturnDetailsLN : ICreateReturnDetailsLN
    {
        private readonly ICreateReturnDetailsAD _createReturnDetailsAD;

        public CreateReturnDetailsLN(ICreateReturnDetailsAD createReturnDetailsAD)
        {
            _createReturnDetailsAD = createReturnDetailsAD;
        }

        public async Task<int> Guardar(ReturnDetailsDto returnDetailsAGuardar)
        {
            int id = await _createReturnDetailsAD.Guardar(returnDetailsAGuardar);
            return id;
        }
    }
}
