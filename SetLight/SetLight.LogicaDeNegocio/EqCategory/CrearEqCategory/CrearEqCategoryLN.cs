using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.EqCategory.CrearEqCategory;
using SetLight.Abstracciones.LogicaDeNegocio.EqCategory.CrearEqCategory;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.EqCategory.CrearEqCategory;

namespace SetLight.LogicaDeNegocio.EqCategory.CrearEqCategory
{
    public class CrearEqCategoryLN : ICrearEqCategoryLN
    {
        private ICrearEqCategoryAD _crearEqCategoryAD;

        public CrearEqCategoryLN()
        {
            _crearEqCategoryAD = new CrearEqCategoryAD();
        }
        public async Task<int> Guardar(EqCategoryDto eqCategoryaGuardar)
        {
            int id = await _crearEqCategoryAD.Guardar(eqCategoryaGuardar);
            return id;
        }
    }
}