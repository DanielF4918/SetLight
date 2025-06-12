using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.Abstracciones.AccesoADatos.RentalOrder.ListarRentalOrder
{
    public interface IListarRentalOrder
    {
        List<RentalOrderDto> Obtener();
    }
}
