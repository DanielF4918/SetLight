using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.EqCategory.CrearEqCategory;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.EqCategory.CrearEqCategory
{
    public class CrearEqCategoryAD : ICrearEqCategoryAD
    {
        private Contexto elContexto;

        public CrearEqCategoryAD()
        {
            elContexto = new Contexto();
        }
        public async Task<int> Guardar(EqCategoryDto eqCategoryaGuardar)
        {
            elContexto.EqCategory.Add(ConvierteEqCategory(eqCategoryaGuardar));

            int resultado = await elContexto.SaveChangesAsync();
            return resultado;
        }

        private EqCategoryDA ConvierteEqCategory(EqCategoryDto eqCategory)
        {
            return new EqCategoryDA
            {
                CategoryId = eqCategory.CategoryId,
                CategoryName = eqCategory.CategoryName,
               
            };
        }
    }
}
