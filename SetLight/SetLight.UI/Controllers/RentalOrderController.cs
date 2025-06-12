using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Clientes.ObtenerClPorID;

namespace SetLight.UI.Controllers
{
    public class RentalOrderController : Controller
    {
        private Contexto _contexto;
        private ObtenerClPorIDAD _obtenerClPorID;

        public RentalOrderController()
        {
            _contexto = new Contexto();
            _obtenerClPorID = new ObtenerClPorIDAD();
        }

        // GET: RentalOrder/History/5
        public ActionResult History(int clientId)
        {
            ClientDto cliente = _obtenerClPorID.Obtener(clientId);

            if (cliente == null)
            {
                return HttpNotFound("Cliente no encontrado");
            }

            // Consultar órdenes con sus detalles y equipos
            var historial = (from orden in _contexto.RentalOrders
                             where orden.ClientId == clientId
                             select new RentalOrderDto
                             {
                                 OrderId = orden.OrderId,
                                 OrderDate = orden.OrderDate,
                                 StartDate = orden.StartDate,
                                 EndDate = orden.EndDate,
                                 StatusOrder = orden.StatusOrder,
                                 ClientId = orden.ClientId,
                                 ClientName = cliente.FirstName + " " + cliente.LastName,

                                 Details = (from detalle in _contexto.OrderDetails
                                            join equipo in _contexto.Equipment
                                            on detalle.EquipmentId equals equipo.EquipmentId
                                            where detalle.OrderId == orden.OrderId
                                            select new OrderDetailDto
                                            {
                                                EquipmentName = equipo.EquipmentName,
                                                Brand = equipo.Brand,
                                                Model = equipo.Model,
                                                RentalValue = equipo.RentalValue,
                                                Quantity = detalle.Quantity
                                            }).ToList()
                             }).ToList();

            ViewBag.ClientName = cliente.FirstName + " " + cliente.LastName;
            return View(historial);
        }


        // GET: RentalOrder
        public ActionResult Index()
        {
            return View();
        }

        // GET: RentalOrder/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RentalOrder/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RentalOrder/Create
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

        // GET: RentalOrder/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RentalOrder/Edit/5
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

        // GET: RentalOrder/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RentalOrder/Delete/5
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
