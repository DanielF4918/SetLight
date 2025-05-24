using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.LogicaDeNegocio.Equipment.ListarEquipment;

namespace SetLight.UI.Controllers
{
    public class EquipmentController : Controller
    {
   private IListarEquipmentLN _listarEquipmentLN;
        private IObtenerEqPorIDLN _ObtenerEqPorIDLN;

        public EquipmentController()
        {
            _listarEquipmentLN = new ListarEquipmentLN();
        }
    
        // GET: Equipment
        public ActionResult ListarEquipment()
        {
            List<EquipmentDto> LaListaEquipment = _listarEquipmentLN.Obtener();
            return View(LaListaEquipment);
        }

        // GET: Equipment/Details/5
        public ActionResult Details(int id)
        {
            List<EquipmentDto> LaListaEquipment = _listarEquipmentLN.Obtener();
            return View(LaListaEquipment);
        }

        // GET: Equipment/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Equipment/Create
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

        // GET: Equipment/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Equipment/Edit/5
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
    }
}
