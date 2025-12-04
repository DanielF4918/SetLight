using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.Abstracciones.AccesoADatos.EqCategory.EditEqCategory;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos.EqCategory.EditEqCategory
{
    public class EditEqCategoryAD: IEditEqCategoryAD
    {

        private Contexto ElContexto;

        public EditEqCategoryAD()
        {
            ElContexto = new Contexto();
        }

        public int Editar(EqCategoryDto EqCategoryParaActualizar)
        {
            EqCategoryDA EqCategoryEnBD = ElContexto.EqCategory.FirstOrDefault(eqCategory => eqCategory.CategoryId == EqCategoryParaActualizar.CategoryId);

            if (EqCategoryEnBD == null)
                return 0;
            EqCategoryEnBD.CategoryName = EqCategoryParaActualizar.CategoryName;

            int seGuardo = ElContexto.SaveChanges();
            return seGuardo;

        }
    }
}
