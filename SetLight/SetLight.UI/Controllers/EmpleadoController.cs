using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.LogicaDeNegocio.Empleado.ListarEmpleado;

namespace SetLight.UI.Controllers
{
    public class EmpleadoController : Controller
    {

        private IListarEmpleadoLN _listarEmpleadoLN;



        public EmpleadoController()
        {
            _listarEmpleadoLN = new ListarEmpleadoLN();
        }
        // GET: Empleado
        public ActionResult ListarEmpleado()
        {
            List<EmpleadoDto> listaEmpleados = _listarEmpleadoLN.Obtener();
            return View(listaEmpleados);
        }

        // GET: Empleado/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Empleado/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Empleado/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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
