using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.LogicaDeNegocio.Empleado.CrearEmpleado;
using SetLight.LogicaDeNegocio.Empleado.ListarEmpleado;
using SetLight.UI.Models; 
using Microsoft.AspNet.Identity.EntityFramework;

namespace SetLight.UI.Controllers
{
    public class EmpleadoController : Controller
    {
        private  IListarEmpleadoLN _listarEmpleadoLN;
        private  ICrearEmpleadoLN _crearEmpleadoLN;
        private  ApplicationDbContext _contexto;

        public EmpleadoController()
        {
            _listarEmpleadoLN = new ListarEmpleadoLN();
            _crearEmpleadoLN = new CrearEmpleadoLN();
            _contexto = new ApplicationDbContext(); 
        }

        // GET: Empleado
        public ActionResult ListarEmpleado()
        {
            List<EmpleadoDto> listaEmpleados = _listarEmpleadoLN.Obtener();
            return View(listaEmpleados);
        }

        // GET: Empleado/Create
        public ActionResult CrearEmpleado()
        {
            ViewBag.Roles = ObtenerListaRoles();
            return View();
        }

        // POST: Empleado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearEmpleado(EmpleadoDto empleadoDto)
        {
            if (ModelState.IsValid)
            {
                await _crearEmpleadoLN.Guardar(empleadoDto);
                return RedirectToAction("ListarEmpleado");
            }

            ViewBag.Roles = ObtenerListaRoles(empleadoDto.RolId);
            return View(empleadoDto);
        }

        private IEnumerable<SelectListItem> ObtenerListaRoles(string rolSeleccionado = null)
        {
            return _contexto.Roles.Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name,
                Selected = r.Id == rolSeleccionado
            }).ToList();
        }

        // GET: Empleado/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Empleado/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Empleado/Edit/5
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

        // GET: Empleado/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Empleado/Delete/5
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
