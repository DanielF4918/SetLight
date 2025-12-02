using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.EqCategory.CrearEqCategory;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.LogicaDeNegocio.EqCategory.CrearEqCategory;
using SetLight.LogicaDeNegocio.EqCategory.EditarEqcategory;

namespace SetLight.UI.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EqCategoryController : Controller
    {
        private ICrearEqCategoryLN _crearEqCategoryLN;

        private readonly EditarEqCategoryLN _editarEqCategoryLN;
        private readonly Contexto _contexto;

        public EqCategoryController()
        {
            _crearEqCategoryLN = new CrearEqCategoryLN();
            _editarEqCategoryLN = new EditarEqCategoryLN();
            _contexto = new Contexto();
        }

        // GET: EqCategory
        public ActionResult Index()
        {
            return View();
        }

        // GET: EqCategory/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EqCategory/CrearEqCategory (llamado por el modal)
        public ActionResult CrearEqCategory()
        {
            return PartialView();
        }

        // POST: EqCategory/CrearEqCategory (usado por AJAX)
        [HttpPost]
        public async Task<ActionResult> CrearEqCategory(EqCategoryDto eqCategoryAguardar)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("CrearEqCategory", eqCategoryAguardar);
            }

            try
            {
                await _crearEqCategoryLN.Guardar(eqCategoryAguardar);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                return PartialView("CrearEqCategory", eqCategoryAguardar);
            }
        }

        // GET: EqCategory/Edit/5
        public ActionResult Edit(int id)
        {
            // Obtenemos la categoría directamente desde el contexto
            var categoriaEnBd = _contexto.EqCategory
                .FirstOrDefault(c => c.CategoryId == id);

            if (categoriaEnBd == null)
            {
                return HttpNotFound();
            }

            var modelo = new EqCategoryDto
            {
                CategoryId = categoriaEnBd.CategoryId,
                CategoryName = categoriaEnBd.CategoryName
            };

            return View(modelo);
        }

        // POST: EqCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EqCategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Usamos la capa de lógica de negocio que ya creaste
            int resultado = _editarEqCategoryLN.Actualizar(model);

            if (resultado == 0)
            {
                // No se encontró o no se guardó
                ModelState.AddModelError("", "No fue posible actualizar la categoría.");
                return View(model);
            }

            TempData["Mensaje"] = "Categoría actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // GET: EqCategory/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EqCategory/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
