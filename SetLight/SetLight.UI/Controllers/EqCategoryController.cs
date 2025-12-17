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
    [Authorize(Roles = "Administrador,Colaborador,Tecnico")]
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


        private bool ExisteNombreCategoria(string nombre, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            var normalizado = nombre.Trim().ToLower();

            var query = _contexto.EqCategory.AsQueryable();

            if (excluirId.HasValue)
            {
                query = query.Where(c => c.CategoryId != excluirId.Value);
            }

            return query.Any(c => c.CategoryName.Trim().ToLower() == normalizado);
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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearEqCategory(EqCategoryDto eqCategoryAguardar)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("CrearEqCategory", eqCategoryAguardar);
            }

            // 🔹 Validar nombre único (crear)
            if (ExisteNombreCategoria(eqCategoryAguardar.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Ya existe una categoría con ese nombre.");
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



        // Lista de categorías en un PartialView (tabla)
        public ActionResult ListarCategoriasPartial(int page = 1)
        {
            int pageSize = 5; // 👈 cantidad por página

            var query = _contexto.EqCategory
                .Select(c => new EqCategoryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .OrderBy(c => c.CategoryName);

            int totalRegistros = query.Count();

            var categorias = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRegistros / pageSize);

            return PartialView("_ListaCategorias", categorias);
        }


        // Formulario de edición en un PartialView
        public ActionResult EditarCategoriaPartial(int id)
        {
            var categoriaEnBd = _contexto.EqCategory
                .FirstOrDefault(c => c.CategoryId == id);

            if (categoriaEnBd == null)
                return HttpNotFound();

            var modelo = new EqCategoryDto
            {
                CategoryId = categoriaEnBd.CategoryId,
                CategoryName = categoriaEnBd.CategoryName
            };

            return PartialView("_EditarCategoria", modelo);
        }

        // POST AJAX para guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCategoriaAjax(EqCategoryDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, mensaje = "Datos inválidos." });
            }

            // 🔹 Validar nombre único (editar)
            if (ExisteNombreCategoria(model.CategoryName, model.CategoryId))
            {
                return Json(new
                {
                    success = false,
                    mensaje = "Ya existe una categoría con ese nombre."
                });
            }

            int resultado = _editarEqCategoryLN.Actualizar(model);

            if (resultado == 0)
            {
                return Json(new { success = false, mensaje = "No fue posible actualizar la categoría." });
            }

            return Json(new { success = true });
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
