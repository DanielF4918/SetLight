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
using SetLight.Entidades;
using SetLight.LogicaDeNegocio.ReturnDetails.CreateReturnDetails;
using System.Data.Entity;
using System.IO;
using SetLight.Entidades.Dto;
using SetLight.AccesoADatos.Modelos;
using PagedList;

namespace SetLight.UI.Controllers
{
    [Authorize(Roles = "Administrador,Tecnico")]
    public class ReturnDetailsController : Controller
    {
        private ICreateReturnDetailsAD _createReturnDetailsAD;
        private readonly Contexto _contexto = new Contexto();

        public ReturnDetailsController()
        {
            _createReturnDetailsAD = new CreateReturnDetailsAD();
        }

        private void CargarCombosMantenimiento(int? equipmentIdSeleccionado = null)
        {
            using (var contexto = new Contexto())
            {
                // 🟢 Combo de equipos
                ViewBag.Equipos = contexto.Equipment
                    .Where(e => e.Status == 1) // sólo activos, si quieres
                    .Select(e => new SelectListItem
                    {
                        Value = e.EquipmentId.ToString(),
                        Text = e.EquipmentName,
                        Selected = (equipmentIdSeleccionado.HasValue &&
                                    equipmentIdSeleccionado.Value == e.EquipmentId)
                    })
                    .ToList();

                // 🟢 Combo de tipos de mantenimiento
                ViewBag.TiposMantenimiento = new[]
                {
                    new SelectListItem { Value = "1", Text = "Correctivo" },
                    new SelectListItem { Value = "2", Text = "Preventivo" },
                    new SelectListItem { Value = "3", Text = "Otro" }
                };
            }
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

        // POST: ReturnDetails/CrearReturnDetails
        [HttpPost]
        public async Task<ActionResult> CrearReturnDetails(EquipmentReturnViewModel model)
        {
            // ✅ Validaciones a nivel de modelo
            if (model.Items != null)
            {
                for (int i = 0; i < model.Items.Count; i++)
                {
                    var item = model.Items[i];

                    int buenas = item.CantidadBuenas;
                    int danadas = item.CantidadDañadas;
                    int faltantes = item.CantidadFaltante;
                    int total = item.Quantity;

                    // 0️⃣ No permitir negativos
                    if (buenas < 0 || danadas < 0 || faltantes < 0)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].CantidadBuenas",
                            "Las cantidades no pueden ser negativas."
                        );
                    }

                    // 1️⃣ Solo pedir MaintenanceType si hay equipos dañados
                    if (danadas > 0 && !item.MaintenanceType.HasValue)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].MaintenanceType",
                            "Debe seleccionar el tipo de mantenimiento cuando hay equipos dañados."
                        );
                    }

                    // 2️⃣ Cada campo individual no puede superar la cantidad alquilada
                    if (buenas > total)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].CantidadBuenas",
                            "La cantidad en buen estado no puede superar la cantidad alquilada."
                        );
                    }

                    if (danadas > total)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].CantidadDañadas",
                            "La cantidad dañada no puede superar la cantidad alquilada."
                        );
                    }

                    if (faltantes > total)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].CantidadFaltante",
                            "La cantidad faltante no puede superar la cantidad alquilada."
                        );
                    }

                    // 3️⃣ La suma total debe igualar la cantidad alquilada
                    int suma = buenas + danadas + faltantes;
                    if (suma != total)
                    {
                        ModelState.AddModelError(
                            $"Items[{i}].CantidadBuenas",
                            "La suma de buenas, dañadas y faltantes debe ser igual a la cantidad alquilada."
                        );
                    }
                }
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var ad = new CreateReturnDetailsAD();
                var ln = new CreateReturnDetailsLN(ad);

                foreach (var item in model.Items)
                {
                    // ✅ 1️⃣ Equipos en buen estado
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
                            await ln.Guardar(dtoBueno);

                        // Actualizar stock de equipos devueltos en buen estado
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

                    // ✅ 2️⃣ Equipos dañados → generan mantenimiento
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
                            await ln.Guardar(dtoDañado);

                        using (var contexto = new Contexto())
                        {
                            // 🔎 Obtenemos el empleado (técnico) a partir del usuario logueado
                            var emailUsuario = User.Identity.Name;   // normalmente es el correo del AspNetUser
                            var empleado = contexto.Empleado
                                .FirstOrDefault(e => e.CorreoElectronico == emailUsuario);

                            // 🔧 Creamos el mantenimiento en estado pendiente
                            var mantenimiento = new Maintenance
                            {
                                StartDate = DateTime.Now,
                                MaintenanceType = item.MaintenanceType.Value, // ya validado
                                MaintenanceStatus = 0, // 0 = Pendiente
                                EquipmentId = item.EquipmentId,
                                Comments = item.Observaciones ?? "Pendiente de revisión",
                                Cost = null,
                                EvidencePath = null,
                                IdEmpleado = empleado?.IdEmpleado   // 👈 técnico responsable
                            };

                            contexto.Maintenance.Add(mantenimiento);
                            contexto.SaveChanges();
                        }
                    }

                    // ✅ 3️⃣ Equipos faltantes / no devueltos
                    if (item.CantidadFaltante > 0)
                    {
                        var dtoFaltante = new ReturnDetailsDto
                        {
                            OrderId = model.OrderId,
                            EquipmentId = item.EquipmentId,
                            ReturnDate = DateTime.Now,
                            ConditionReport = item.Observaciones ?? "Equipo no devuelto / perdido",
                            IsReturned = false,
                            RequiresMaintenance = false
                        };

                        for (int i = 0; i < item.CantidadFaltante; i++)
                            await ln.Guardar(dtoFaltante);

                        // No se suma al stock
                    }
                }

                // ✅ 4️⃣ Verificar si la orden quedó completamente gestionada
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
                            orden.StatusOrder = 2; // 2 = Finalizada
                            contexto.SaveChanges();
                        }
                    }
                }

                TempData["Success"] = "Devolución registrada correctamente.";
                return RedirectToAction("Index", "RentalOrder");
            }
            catch (Exception ex)
            {
                var mensaje = ex.Message;
                if (ex.InnerException != null)
                    mensaje += " - " + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null)
                    mensaje += " - " + ex.InnerException.InnerException.Message;

                ModelState.AddModelError("", "Error al guardar devoluciones: " + mensaje);
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

        // Listado de mantenimientos con filtros + paginación
        // GET: /ReturnDetails/Mantenimientos
        public ActionResult Mantenimientos(
            string equipo,
            int? tipo,
            int? estado,
            DateTime? desde,
            DateTime? hasta,
            int? page
        )
        {
            using (var contexto = new Contexto())
            {
                var q = contexto.Maintenance
                    .Include(m => m.Equipment)
                    .AsQueryable();

                // Filtros opcionales
                if (!string.IsNullOrWhiteSpace(equipo))
                {
                    var term = equipo.Trim().ToLower();
                    q = q.Where(m => (m.Equipment.EquipmentName ?? "").ToLower().Contains(term));
                }

                if (tipo.HasValue)
                    q = q.Where(m => m.MaintenanceType == tipo.Value);

                if (estado.HasValue)
                    q = q.Where(m => m.MaintenanceStatus == estado.Value);

                if (desde.HasValue)
                    q = q.Where(m => m.StartDate >= desde.Value);

                if (hasta.HasValue)
                    q = q.Where(m => m.StartDate <= hasta.Value);

                // Orden: más recientes primero, luego ID desc
                q = q.OrderByDescending(m => m.StartDate)
                     .ThenByDescending(m => m.MaintenanceId);

                // Paginación
                int pageSize = 12;              // cantidad de cards por página (ajustable)
                int pageNumber = page ?? 1;

                var listaPaginada = q.ToPagedList(pageNumber, pageSize);

                // Mantener valores de filtros para la vista y la paginación
                ViewBag.FiltroEquipo = equipo;
                ViewBag.FiltroTipo = tipo;
                ViewBag.FiltroEstado = estado;
                ViewBag.FiltroDesde = desde?.ToString("yyyy-MM-dd");
                ViewBag.FiltroHasta = hasta?.ToString("yyyy-MM-dd");

                return View(listaPaginada);
            }
        }


        // GET: ReturnDetails/Finalize/5
        public ActionResult Finalize(int id)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance
                    .Include(m => m.Equipment)
                    .FirstOrDefault(m => m.MaintenanceId == id);

                if (mantenimiento == null)
                    return HttpNotFound();

                return View(mantenimiento);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizarMantenimiento(int id, string comments, decimal? cost, HttpPostedFileBase evidenceFile)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance.Find(id);
                if (mantenimiento == null)
                    return HttpNotFound();

                // Guardar evidencia si hay archivo nuevo
                if (evidenceFile != null && evidenceFile.ContentLength > 0)
                {
                    var evidenciasRoot = Server.MapPath("~/Evidencias/");
                    Directory.CreateDirectory(evidenciasRoot);

                    var originalName = System.IO.Path.GetFileName(evidenceFile.FileName);
                    var extension = System.IO.Path.GetExtension(originalName);
                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    var fullPath = Path.Combine(evidenciasRoot, fileName);

                    evidenceFile.SaveAs(fullPath);
                    mantenimiento.EvidencePath = "/Evidencias/" + fileName;
                }

                mantenimiento.Comments = comments;
                mantenimiento.Cost = cost;
                mantenimiento.MaintenanceStatus = 1;         // 1 = Finalizado
                mantenimiento.EndDate = DateTime.Now;

                mantenimiento.FinalizadoPor = Session["NombreUsuario"]?.ToString()
                                              ?? User.Identity.Name;

                // 🔹 Actualizar Stock: una unidad vuelve a estar disponible
                var equipo = contexto.Equipment.Find(mantenimiento.EquipmentId);
                if (equipo != null)
                {
                    equipo.Stock += 1;
                }

                contexto.SaveChanges();
            }

            TempData["Success"] = "Mantenimiento finalizado correctamente.";
            return RedirectToAction("Mantenimientos");
        }



        public ActionResult TestInsertarMantenimiento()
        {
            using (var ctx = new Contexto())
            {
                ctx.Maintenance.Add(new Maintenance
                {
                    StartDate = DateTime.Now,
                    MaintenanceType = 1,
                    MaintenanceStatus = 0,
                    EquipmentId = 1
                });
                ctx.SaveChanges();
            }
            return Content("¡Inserción de prueba completada!");
        }

        public ActionResult Historico()
        {
            using (var contexto = new Contexto())
            {
                var listaHistorico = contexto.Maintenance
                    .Include(m => m.Equipment)
                    .Where(m => m.MaintenanceStatus == 1 || m.MaintenanceStatus == 2)
                    .OrderByDescending(m => m.StartDate)
                    .ToList();

                return View(listaHistorico);
            }
        }

        [HttpGet]
        public ActionResult DetallesMantenimiento(int id)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = (
                    from m in contexto.Maintenance
                    join eq in contexto.Equipment
                        on m.EquipmentId equals eq.EquipmentId
                    join emp in contexto.Empleado
                        on m.IdEmpleado equals emp.IdEmpleado into empJoin
                    from emp in empJoin.DefaultIfEmpty()
                    where m.MaintenanceId == id
                    select new MaintenanceDto
                    {
                        MaintenanceId = m.MaintenanceId,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate,
                        MaintenanceType = m.MaintenanceType,
                        MaintenanceStatus = m.MaintenanceStatus,
                        EquipmentId = m.EquipmentId,
                        EquipmentName = eq.EquipmentName,
                        Comments = m.Comments,
                        Cost = m.Cost,
                        EvidencePath = m.EvidencePath,
                        IdEmpleado = m.IdEmpleado,
                        TechnicianName = emp != null
                            ? emp.Nombre + " " + emp.Apellido
                            : null,
                        FinalizadoPor = m.FinalizadoPor
                    }
                ).FirstOrDefault();

                if (mantenimiento == null)
                    return HttpNotFound();

                return View(mantenimiento);
            }
        }

        // GET: ReturnDetails/EditarMantenimiento/5
        public ActionResult EditarMantenimiento(int id)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance
                    .Include(m => m.Equipment)
                    .FirstOrDefault(m => m.MaintenanceId == id);

                if (mantenimiento == null)
                    return HttpNotFound();

                return View(mantenimiento);
            }
        }

        // POST: ReturnDetails/EditarMantenimiento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMantenimiento(int id, string comments, decimal? cost, HttpPostedFileBase evidenceFile)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance.Find(id);
                if (mantenimiento == null)
                    return HttpNotFound();

                // Guardar evidencia si hay archivo nuevo
                if (evidenceFile != null && evidenceFile.ContentLength > 0)
                {
                    var fileName = System.IO.Path.GetFileName(evidenceFile.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Evidencias/"), fileName);
                    evidenceFile.SaveAs(path);
                    mantenimiento.EvidencePath = "/Evidencias/" + fileName;
                }

                mantenimiento.Comments = comments;
                mantenimiento.Cost = cost;

                contexto.SaveChanges();
            }

            TempData["Success"] = "Mantenimiento actualizado correctamente.";
            return RedirectToAction("Mantenimientos");
        }

        // GET: ReturnDetails/CreateMaintenance
        [HttpGet]

        public ActionResult CreateMaintenance()
        {
            using (var contexto = new Contexto())
            {
                var equipos = contexto.Equipment
                    .Where(e => e.Status == 1 && e.Stock > 0)   // 👈 AQUÍ el cambio

                    .Select(e => new EquipmentDto
                    {
                        EquipmentId = e.EquipmentId,
                        EquipmentName = e.EquipmentName,
                        Brand = e.Brand,
                        Model = e.Model,
                        Stock = e.Stock
                    })
                    .ToList();

                var model = new CrearMaintenanceViewModel
                {
                    Equipos = equipos
                };

                return View(model);
            }
        }


        // POST: ReturnDetails/CreateMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMaintenance(CrearMaintenanceViewModel model, HttpPostedFileBase evidenceFile)
        {
            // 🔹 Validación de cantidad
            if (model.Cantidad <= 0)
            {
                ModelState.AddModelError("Cantidad", "La cantidad debe ser al menos 1.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombosMantenimiento(model.EquipmentId);
                return View("CreateMaintenance", model);   // 👈 devolvemos el ViewModel correcto
            }

            if (model.StartDate == default(DateTime))
                model.StartDate = DateTime.Today;

            using (var contexto = new Contexto())
            {
                // Técnico logueado
                var emailUsuario = User.Identity.Name;
                var empleado = contexto.Empleado
                    .FirstOrDefault(e => e.CorreoElectronico == emailUsuario);

                var equipo = contexto.Equipment
                    .FirstOrDefault(e => e.EquipmentId == model.EquipmentId);

                if (equipo == null)
                {
                    ModelState.AddModelError("EquipmentId", "El equipo seleccionado no existe.");
                    CargarCombosMantenimiento(model.EquipmentId);
                    return View("CreateMaintenance", model);
                }

                // 🔹 Validar que haya suficientes unidades disponibles (usando Stock)
                if (equipo.Stock < model.Cantidad)
                {
                    // Mensaje claro, parecido al de las órdenes
                    var msg = $"No hay suficientes unidades disponibles para el equipo: {equipo.EquipmentName}. " +
                              $"Disponibles: {equipo.Stock}, seleccionadas: {model.Cantidad}.";

                    ModelState.AddModelError(string.Empty, msg);   // aparece en ValidationSummary
                    ModelState.AddModelError("Cantidad", msg);     // aparece junto al campo Cantidad

                    CargarCombosMantenimiento(model.EquipmentId);
                    return View("CreateMaintenance", model);
                }

                // 🔹 Manejo de evidencia (solo se guarda el archivo una vez)
                string evidencePath = null;
                if (evidenceFile != null && evidenceFile.ContentLength > 0)
                {
                    var evidenciasRoot = Server.MapPath("~/Evidencias/");
                    Directory.CreateDirectory(evidenciasRoot);

                    var originalName = Path.GetFileName(evidenceFile.FileName);
                    var extension = Path.GetExtension(originalName);
                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    var fullPath = Path.Combine(evidenciasRoot, fileName);

                    evidenceFile.SaveAs(fullPath);
                    evidencePath = "/Evidencias/" + fileName;
                }

                // 🔹 Crear N mantenimientos (uno por unidad)
                for (int i = 0; i < model.Cantidad; i++)
                {
                    var mantenimiento = new Maintenance
                    {
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        MaintenanceType = model.MaintenanceType,
                        MaintenanceStatus = 0,           // 0 = Pendiente
                        EquipmentId = model.EquipmentId,
                        Comments = model.Comments,
                        Cost = model.Cost,
                        IdEmpleado = empleado?.IdEmpleado,
                        EvidencePath = evidencePath
                    };

                    contexto.Maintenance.Add(mantenimiento);
                }

                // 🔹 Actualizar Stock (disponibles)
                equipo.Stock -= model.Cantidad;

                contexto.SaveChanges();
            }

            TempData["Success"] = "Mantenimientos creados correctamente.";
            return RedirectToAction("Mantenimientos");
        }



    }
}
