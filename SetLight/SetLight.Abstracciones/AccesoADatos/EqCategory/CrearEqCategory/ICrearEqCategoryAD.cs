using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.ModelosParaUI;

namespace SetLight.Abstracciones.AccesoADatos.EqCategory.CrearEqCategory
{
    public interface ICrearEqCategoryAD
    {
        Task<int> Guardar(EqCategoryDto eqCategoryAguardar);
    }
}
