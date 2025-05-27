using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.EqCategory.CrearEqCategory;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.LogicaDeNegocio.EqCategory.CrearEqCategory;

namespace SetLight.UI.Controllers
{
    public class EqCategoryController : Controller
    {
        private ICrearEqCategoryLN _crearEqCategoryLN;

        public EqCategoryController()
        {
            _crearEqCategoryLN = new CrearEqCategoryLN();
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
            return View();
        }

        // POST: EqCategory/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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
