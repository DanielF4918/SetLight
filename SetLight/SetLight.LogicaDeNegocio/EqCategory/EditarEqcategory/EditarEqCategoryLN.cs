using SetLight.Abstracciones.AccesoADatos.EqCategory.EditEqCategory;
using SetLight.Abstracciones.LogicaDeNegocio.EqCategory.EditEqCategory;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.EqCategory.EditEqCategory;

namespace SetLight.LogicaDeNegocio.EqCategory.EditarEqcategory
{
    public class EditarEqCategoryLN : IEditEqCategoryLN
    {
        private IEditEqCategoryAD _actualizarEqCategory;

        public EditarEqCategoryLN()
        {
            _actualizarEqCategory = new EditEqCategoryAD();
        }

        public int Actualizar(EqCategoryDto eqCategoryParaEditar)
        {
            return _actualizarEqCategory.Editar(eqCategoryParaEditar);
        }
    }
}
