using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.AccesoADatos.ReturnDetails.CreateReturnDetails;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.Abstracciones.ViewModels;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.ReturnDetails.CreateReturnDetails;
using SetLight.LogicaDeNegocio.ReturnDetails.CreateReturnDetails;

namespace SetLight.UI.Controllers
{
    public class ReturnDetailsController : Controller
    {
        private ICreateReturnDetailsAD _createReturnDetailsAD;

        public ReturnDetailsController()
        {
            _createReturnDetailsAD = new CreateReturnDetailsAD();
        }
        public ActionResult DetallesDevolucion(int orderId)
        {
            using (var contexto = new Contexto())
            {
                var devoluciones = contexto.ReturnDetails
                    .Include("Equipment")
                    .Where(d => d.OrderId == orderId)
                    .ToList();

                if (!devoluciones.Any())
                    return HttpNotFound("No hay devoluciones registradas para esta orden.");

                var viewModel = devoluciones.Select(d => new ReturnDetailsDto
                {
                    EquipmentName = d.Equipment.EquipmentName,
                    ReturnDate = d.ReturnDate,
                    ConditionReport = d.ConditionReport,
                    IsReturned = d.IsReturned,
                    RequiresMaintenance = d.RequiresMaintenance
                }).ToList();

                ViewBag.OrderId = orderId;
                return View(viewModel);
            }
        }



        // GET: ReturnDetails/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReturnDetails/CrearReturnDetails
        public ActionResult CrearReturnDetails(int orderId)
        {
            using (var contexto = new Contexto())
            {
                var orden = contexto.RentalOrders
                    .Include("OrderDetails.Equipment")
                    .Include("Client")
                    .FirstOrDefault(o => o.OrderId == orderId);

                if (orden == null) return HttpNotFound();

                var model = new EquipmentReturnViewModel
                {
                    OrderId = orden.OrderId,
                    ClientName = orden.Client.FirstName + " " + orden.Client.LastName,
                    Items = orden.OrderDetails.Select(od => new EquipmentReturnItem
                    {
                        EquipmentId = od.EquipmentId,
                        EquipmentName = od.Equipment.EquipmentName,
                        Quantity = od.Quantity,
                        CantidadBuenas = 0,
                        CantidadDañadas = 0,
                        Observaciones = ""
                    }).ToList()
                };

                return View(model);
            }
        }



        //POST: ReturnDetails/CrearReturnDetails
        [HttpPost]
        public async Task<ActionResult> CrearReturnDetails(EquipmentReturnViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var ad = new CreateReturnDetailsAD();
                var ln = new CreateReturnDetailsLN(ad);

                foreach (var item in model.Items)
                {
                    if (item.CantidadBuenas > 0)
                    {
                        var dtoBueno = new ReturnDetailsDto
                        {
                            OrderId = model.OrderId,
                            EquipmentId = item.EquipmentId,
                            ReturnDate = DateTime.Now,
                            ConditionReport = "Buen estado",
                            IsReturned = true,
                            RequiresMaintenance = false
                        };

                        for (int i = 0; i < item.CantidadBuenas; i++)
                        {
                            await ln.Guardar(dtoBueno);
                        }

                        using (var contextoStock = new Contexto())
                        {
                            var equipo = contextoStock.Equipment.FirstOrDefault(e => e.EquipmentId == item.EquipmentId);
                            if (equipo != null)
                            {
                                equipo.Stock += item.CantidadBuenas;
                                contextoStock.SaveChanges();
                            }
                        }
                    }

                    if (item.CantidadDañadas > 0)
                    {
                        var dtoDañado = new ReturnDetailsDto
                        {
                            OrderId = model.OrderId,
                            EquipmentId = item.EquipmentId,
                            ReturnDate = DateTime.Now,
                            ConditionReport = item.Observaciones ?? "Equipo dañado",
                            IsReturned = true,
                            RequiresMaintenance = true
                        };

                        for (int i = 0; i < item.CantidadDañadas; i++)
                        {
                            await ln.Guardar(dtoDañado);
                        }

                    }
                }

                using (var contexto = new Contexto())
                {
                    var orderDetails = contexto.OrderDetails
                        .Where(od => od.OrderId == model.OrderId)
                        .ToList();

                    var returnCountPorEquipo = contexto.ReturnDetails
                        .Where(rd => rd.OrderId == model.OrderId)
                        .GroupBy(rd => rd.EquipmentId)
                        .Select(g => new { EquipmentId = g.Key, TotalDevueltos = g.Count() })
                        .ToDictionary(x => x.EquipmentId, x => x.TotalDevueltos);

                    bool ordenCompletada = true;

                    foreach (var detalle in orderDetails)
                    {
                        if (!returnCountPorEquipo.TryGetValue(detalle.EquipmentId, out int devueltos) || devueltos < detalle.Quantity)
                        {
                            ordenCompletada = false;
                            break;
                        }
                    }

                    if (ordenCompletada)
                    {
                        var orden = contexto.RentalOrders.FirstOrDefault(o => o.OrderId == model.OrderId);
                        if (orden != null)
                        {
                            orden.StatusOrder = 2; 
                            contexto.SaveChanges();
                        }
                    }
                }

                return RedirectToAction("Index", "RentalOrder");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar devoluciones: " + ex.Message);
                return View(model);
            }
        }






        // GET: ReturnDetails/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReturnDetails/Edit/5
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

        // GET: ReturnDetails/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReturnDetails/Delete/5
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
