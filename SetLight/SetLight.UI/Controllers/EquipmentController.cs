using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.AccesoADatos.Equipment.CrearEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.CrearEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.EditarEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.LogicaDeNegocio.Equipment.CrearEquipment;
using SetLight.LogicaDeNegocio.Equipment.EditarEquipment;
using SetLight.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.LogicaDeNegocio.Equipment.ObtenerEqPorID;

namespace SetLight.UI.Controllers
{
    public class EquipmentController : Controller
    {
        private IListarEquipmentLN _listarEquipmentLN;
        private IObtenerEqPorIDLN _ObtenerEqPorIDLN;
        private ICrearEquipmentLN _crearEquipmentLN;
        private IEquipmentLN _equipmentLN;

        public EquipmentController()
        {
            _listarEquipmentLN = new ListarEquipmentLN();
            _crearEquipmentLN = new CrearEquipmentLN();
            _ObtenerEqPorIDLN = new ObtenerEqPorIDLN();
            _equipmentLN = new EditarEquipmentLN();
        }

        // GET: Equipment
        public ActionResult ListarEquipment(string Nombre, int? CategoriaId, int? Estado)
        {
            var lista = _listarEquipmentLN.Obtener();

            // Filtros
            if (!string.IsNullOrWhiteSpace(Nombre))
                lista = lista.Where(e => e.EquipmentName != null &&
                                         e.EquipmentName.ToLower().Contains(Nombre.ToLower())).ToList();

            if (CategoriaId.HasValue && CategoriaId.Value != 0)
                lista = lista.Where(e => e.CategoryId == CategoriaId).ToList();

            if (Estado.HasValue && Estado.Value != 0)
                lista = lista.Where(e => e.Status == Estado).ToList();

            // ViewBags para combos
            using (var contexto = new Contexto())
            {
                ViewBag.Categorias = contexto.EqCategory
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName,
                        Selected = (CategoriaId.HasValue && CategoriaId == c.CategoryId)
                    }).ToList();
            }

            ViewBag.Estados = new List<SelectListItem>
    {
        new SelectListItem { Value = "0", Text = "Todos", Selected = Estado == null || Estado == 0 },
        new SelectListItem { Value = "1", Text = "Activo", Selected = Estado == 1 },
        new SelectListItem { Value = "2", Text = "Agotado", Selected = Estado == 2 },
        new SelectListItem { Value = "3", Text = "Inactivo", Selected = Estado == 3 }
    };

            ViewBag.NombreBuscado = Nombre;

            return View(lista);
        }

        // GET: Equipment/Details/5
        public ActionResult Details(int id)
        {
            List<EquipmentDto> LaListaEquipment = _listarEquipmentLN.Obtener();
            return View(LaListaEquipment);
        }

        // GET: Equipment/Create
        public ActionResult CrearEquipment()
        {
            using (var contexto = new Contexto())
            {
                var categorias = contexto.EqCategory
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName
                    }).ToList();

                ViewBag.Categorias = categorias;
            }

            return View();
        }


        // POST: Equipment/Create
        [HttpPost]
        public async Task<ActionResult> CrearEquipment(EquipmentDto equipmentguardar)
        {
            if (!ModelState.IsValid)
                return View(equipmentguardar);

            try
            {
                await _crearEquipmentLN.Guardar(equipmentguardar);
                return RedirectToAction("ListarEquipment");
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                return View(equipmentguardar);
            }
        }


        // GET: Equipment/Edit/5
        public ActionResult Edit(int id)
        {
            EquipmentDto elEquipment = _ObtenerEqPorIDLN.Obtener(id);

            using (var contexto = new Contexto())
            {
                var categorias = contexto.EqCategory
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName,
                        Selected = (c.CategoryId == elEquipment.CategoryId)
                    }).ToList();

                ViewBag.Categorias = categorias;
            }

            return View("EditEquipment", elEquipment);
        }

        // POST: Equipment/Edit/5
        [HttpPost]
        public ActionResult Edit(EquipmentDto elEquipment)
        {
            try
            {
                int seGuardo = _equipmentLN.Actualizar(elEquipment);
                return RedirectToAction("ListarEquipment");
            }
            catch
            {
                return View(elEquipment);
            }
        }

        // GET: Equipment/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Equipment/Delete/5
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
        // GET: Equipment/Activar/5
        public ActionResult Activar(int id)
        {
            var equipo = _ObtenerEqPorIDLN.Obtener(id);
            equipo.Status = 1; // Activo
            _equipmentLN.Actualizar(equipo);
            return RedirectToAction("ListarEquipment");
        }

        // GET: Equipment/Inactivar/5
        public ActionResult Inactivar(int id)
        {
            var equipo = _ObtenerEqPorIDLN.Obtener(id);
            equipo.Status = 3; // Inactivo
            _equipmentLN.Actualizar(equipo);
            return RedirectToAction("ListarEquipment");
        }
    }
}
