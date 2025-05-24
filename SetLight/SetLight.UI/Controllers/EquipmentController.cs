using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.AccesoADatos.Equipment.CrearEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.CrearEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.LogicaDeNegocio.Equipment.CrearEquipment;
using SetLight.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.LogicaDeNegocio.Equipment.ObtenerEqPorID;

namespace SetLight.UI.Controllers
{
    public class EquipmentController : Controller
    {
   private IListarEquipmentLN _listarEquipmentLN;
        private IObtenerEqPorIDLN _ObtenerEqPorIDLN;
        private ICrearEquipmentLN _crearEquipmentLN;

        public EquipmentController()
        {
            _listarEquipmentLN = new ListarEquipmentLN();
            _crearEquipmentLN = new CrearEquipmentLN();
            _ObtenerEqPorIDLN = new ObtenerEqPorIDLN();
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
        public ActionResult CrearEquipment()
        {
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
