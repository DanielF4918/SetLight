using SetLight.Abstracciones.ModelosParaUI;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.LogicaDeNegocio.RentalOrder.CrearRentalOrder
{
    public interface ICrearRentalOrderLN
    {
        Task<int> Guardar(RentalOrderDto orden);
    }
}

