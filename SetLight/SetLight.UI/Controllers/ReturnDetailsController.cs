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
    [Authorize(Roles = "Administrador,Colaborador,Tecnico")]
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
                // 1. Obtener la orden
                var orden = contexto.RentalOrders
                    .FirstOrDefault(o => o.OrderId == orderId);

                if (orden == null)
                    return HttpNotFound("No se encontró la orden especificada.");

                // 2. Obtener el cliente asociado
                var cliente = contexto.Clients
                    .FirstOrDefault(c => c.ClientId == orden.ClientId);

                string clienteNombre = cliente != null
                    ? string.Format("{0} {1}", cliente.FirstName ?? "", cliente.LastName ?? "").Trim()
                    : "Sin cliente";

                // ✅ 3. Precio pactado por equipo (snapshot)
                var preciosPactados = contexto.OrderDetails
                    .Where(od => od.OrderId == orderId)
                    .Select(od => new { od.EquipmentId, od.UnitRentalPrice })
                    .ToList()
                    .GroupBy(x => x.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.First().UnitRentalPrice);

                // 4. Cargar devoluciones de la orden
                var devoluciones = contexto.ReturnDetails
                    .Include("Equipment")
                    .Where(d => d.OrderId == orderId)
                    .ToList();

                // 5. Mapear a DTO
                var viewModel = devoluciones.Select(d =>
                {
                    preciosPactados.TryGetValue(d.EquipmentId, out var precioPactado);

                    return new ReturnDetailsDto
                    {
                        ReturnDetailId = d.ReturnDetailId,
                        OrderId = d.OrderId,
                        EquipmentId = d.EquipmentId,

                        EquipmentName = d.Equipment != null ? d.Equipment.EquipmentName : "",

                        ReturnDate = d.ReturnDate,
                        ConditionReport = d.ConditionReport,
                        IsReturned = d.IsReturned,
                        RequiresMaintenance = d.RequiresMaintenance,

                        ClientId = cliente != null ? cliente.ClientId : 0,
                        ClientName = clienteNombre,

                        // ✅ aquí va el snapshot
                        UnitRentalPrice = precioPactado
                    };
                }).ToList();

                // 6. Datos para el resumen de la orden en la vista (cabecera)
                ViewBag.OrderId = orden.OrderId;
                ViewBag.ClientName = clienteNombre;
                ViewBag.ClientId = cliente != null ? (int?)cliente.ClientId : null;

                ViewBag.StartDate = orden.StartDate;
                ViewBag.EndDate = orden.EndDate;

                ViewBag.StatusTexto = orden.StatusOrder == 1
                    ? "Activa"
                    : orden.StatusOrder == 2
                        ? "Completada"
                        : "Desconocido";

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

                // 🔒 BLINDAJE: solo Activa (1) puede registrar devolución
                if (orden.StatusOrder != 1)
                {
                    TempData["Error"] = "No se puede registrar devolución: la orden ya está completada o cancelada.";
                    return RedirectToAction("Index", "RentalOrder");
                }

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
                        CantidadFaltante = 0,
                        Observaciones = "",
                        MaintenanceType = null
                    }).ToList()
                };

                return View(model);
            }
        }


        // POST: ReturnDetails/CrearReturnDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearReturnDetails(EquipmentReturnViewModel model)
        {
            // Helper: rehidratar el model desde BD (manteniendo lo que el usuario escribió)
            EquipmentReturnViewModel RehidratarModel(int orderId, EquipmentReturnViewModel posted = null)
            {
                using (var ctx = new Contexto())
                {
                    var ordenDb = ctx.RentalOrders
                        .Include("OrderDetails.Equipment")
                        .Include("Client")
                        .FirstOrDefault(o => o.OrderId == orderId);

                    if (ordenDb == null) return null;

                    var map = (posted?.Items ?? new List<EquipmentReturnItem>())
                        .ToDictionary(x => x.EquipmentId, x => x);

                    var vm = new EquipmentReturnViewModel
                    {
                        OrderId = ordenDb.OrderId,
                        ClientName = ordenDb.Client.FirstName + " " + ordenDb.Client.LastName,
                        Items = ordenDb.OrderDetails.Select(od =>
                        {
                            map.TryGetValue(od.EquipmentId, out var it);

                            return new EquipmentReturnItem
                            {
                                EquipmentId = od.EquipmentId,
                                EquipmentName = od.Equipment.EquipmentName,
                                Quantity = od.Quantity,

                                CantidadBuenas = it?.CantidadBuenas ?? 0,
                                CantidadDañadas = it?.CantidadDañadas ?? 0,
                                CantidadFaltante = it?.CantidadFaltante ?? 0,

                                Observaciones = it?.Observaciones ?? "",
                                MaintenanceType = it?.MaintenanceType
                            };
                        }).ToList()
                    };

                    return vm;
                }
            }

            // 🔒 BLINDAJE CRÍTICO (anti URL / anti POST manual)
            using (var contexto = new Contexto())
            {
                var orden = contexto.RentalOrders.FirstOrDefault(o => o.OrderId == model.OrderId);
                if (orden == null) return HttpNotFound();

                if (orden.StatusOrder != 1)
                {
                    TempData["Error"] = "No se puede registrar devolución: la orden ya no está activa.";
                    return RedirectToAction("Index", "RentalOrder");
                }
            }

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

                    if (buenas < 0 || danadas < 0 || faltantes < 0)
                    {
                        ModelState.AddModelError($"Items[{i}].CantidadBuenas", "Las cantidades no pueden ser negativas.");
                    }

                    if (danadas > 0 && !item.MaintenanceType.HasValue)
                    {
                        ModelState.AddModelError($"Items[{i}].MaintenanceType",
                            "Debe seleccionar el tipo de mantenimiento cuando hay equipos dañados.");
                    }

                    if (buenas > total)
                        ModelState.AddModelError($"Items[{i}].CantidadBuenas", "La cantidad en buen estado no puede superar la cantidad alquilada.");

                    if (danadas > total)
                        ModelState.AddModelError($"Items[{i}].CantidadDañadas", "La cantidad dañada no puede superar la cantidad alquilada.");

                    if (faltantes > total)
                        ModelState.AddModelError($"Items[{i}].CantidadFaltante", "La cantidad faltante no puede superar la cantidad alquilada.");

                    int suma = buenas + danadas + faltantes;
                    if (suma != total)
                    {
                        ModelState.AddModelError($"Items[{i}].CantidadBuenas",
                            "La suma de buenas, dañadas y faltantes debe ser igual a la cantidad alquilada.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                var re = RehidratarModel(model.OrderId, model);
                if (re == null) return HttpNotFound();
                return View(re);
            }

            try
            {
                var ad = new CreateReturnDetailsAD();
                var ln = new CreateReturnDetailsLN(ad);

                foreach (var item in model.Items)
                {
                    // ✅ 1️⃣ Buen estado
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

                        using (var ctxStock = new Contexto())
                        {
                            var equipo = ctxStock.Equipment.FirstOrDefault(e => e.EquipmentId == item.EquipmentId);
                            if (equipo != null)
                            {
                                equipo.Stock += item.CantidadBuenas;
                                ctxStock.SaveChanges();
                            }
                        }
                    }

                    // ✅ 2️⃣ Dañados -> mantenimiento
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
                            var emailUsuario = User.Identity.Name;
                            var empleado = contexto.Empleado.FirstOrDefault(e => e.CorreoElectronico == emailUsuario);

                            var mantenimiento = new Maintenance
                            {
                                StartDate = DateTime.Now,
                                MaintenanceType = item.MaintenanceType.Value,
                                MaintenanceStatus = 0,
                                EquipmentId = item.EquipmentId,
                                OrderId = model.OrderId,
                                Comments = item.Observaciones ?? "Pendiente de revisión",
                                Cost = null,
                                EvidencePath = null,
                                IdEmpleado = empleado?.IdEmpleado
                            };

                            contexto.Maintenance.Add(mantenimiento);
                            contexto.SaveChanges();
                        }
                    }

                    // ✅ 3️⃣ Faltantes
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

                        using (var contexto = new Contexto())
                        {
                            var emailUsuario = User.Identity.Name;
                            var empleado = contexto.Empleado.FirstOrDefault(e => e.CorreoElectronico == emailUsuario);

                            var mantenimientoFaltante = new Maintenance
                            {
                                StartDate = DateTime.Now,
                                EndDate = null,
                                MaintenanceType = 4,
                                MaintenanceStatus = 0,
                                EquipmentId = item.EquipmentId,
                                OrderId = model.OrderId,
                                Comments = item.Observaciones ?? "Equipo no devuelto / perdido",
                                Cost = null,
                                EvidencePath = null,
                                IdEmpleado = empleado?.IdEmpleado
                            };

                            contexto.Maintenance.Add(mantenimientoFaltante);
                            contexto.SaveChanges();
                        }
                    }
                }

                // ✅ 4️⃣ Completar orden si ya se gestionó todo (devuelto o marcado faltante)
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
                            orden.StatusOrder = 2; // Completada
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
                if (ex.InnerException != null) mensaje += " - " + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null) mensaje += " - " + ex.InnerException.InnerException.Message;

                ModelState.AddModelError("", "Error al guardar devoluciones: " + mensaje);

                var re = RehidratarModel(model.OrderId, model);
                if (re == null) return HttpNotFound();
                return View(re);
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
                    // 👇 Excluir siempre los "no devueltos" (tipo 4) del listado de mantenimiento
                    .Where(m => m.MaintenanceType != 4)
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

                // 🔒 BLINDAJE: solo se puede finalizar si está Pendiente (0)
                if (mantenimiento.MaintenanceStatus != 0)
                {
                    TempData["Error"] = "Este mantenimiento ya fue finalizado o no está disponible para finalizar.";
                    return RedirectToAction("Mantenimientos");
                }

                // 🔒 (opcional) si NO querés permitir finalizar faltantes aquí (tipo 4)
                if (mantenimiento.MaintenanceType == 4)
                {
                    TempData["Error"] = "Este registro corresponde a un faltante y no se finaliza desde este flujo.";
                    return RedirectToAction("Mantenimientos");
                }

                return View(mantenimiento);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizarMantenimiento(int MaintenanceId,
    string comments,
    decimal? cost,
    HttpPostedFileBase evidenceFile)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance.Find(MaintenanceId);
                if (mantenimiento == null)
                    return HttpNotFound();

                // 🔒 BLINDAJE CRÍTICO: si ya no está pendiente, NO permitir finalizar (anti POST manual + anti doble click)
                if (mantenimiento.MaintenanceStatus != 0)
                {
                    TempData["Error"] = "No se puede finalizar: este mantenimiento ya fue procesado.";
                    return RedirectToAction("Mantenimientos");
                }

                // 🔒 (opcional) bloquear faltantes aquí
                if (mantenimiento.MaintenanceType == 4)
                {
                    TempData["Error"] = "No se puede finalizar un faltante desde este flujo.";
                    return RedirectToAction("Mantenimientos");
                }

                // Guardar evidencia si hay archivo nuevo
                if (evidenceFile != null && evidenceFile.ContentLength > 0)
                {
                    var evidenciasRoot = Server.MapPath("~/Evidencias/");
                    Directory.CreateDirectory(evidenciasRoot);

                    var originalName = Path.GetFileName(evidenceFile.FileName);
                    var extension = Path.GetExtension(originalName);

                    // (opcional) validar extensión
                    // var allowed = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                    // if (!allowed.Contains(extension.ToLower())) { ... }

                    var fileName = $"{Guid.NewGuid():N}{extension}";
                    var fullPath = Path.Combine(evidenciasRoot, fileName);

                    evidenceFile.SaveAs(fullPath);
                    mantenimiento.EvidencePath = "/Evidencias/" + fileName;
                }

                mantenimiento.Comments = comments;
                mantenimiento.Cost = cost;
                mantenimiento.MaintenanceStatus = 1; // 1 = Finalizado
                mantenimiento.EndDate = DateTime.Now;

                mantenimiento.FinalizadoPor = Session["NombreUsuario"]?.ToString()
                                              ?? User.Identity.Name;

                // 🔹 Actualizar Stock: una unidad vuelve a estar disponible
                var equipo = contexto.Equipment.Find(mantenimiento.EquipmentId);
                if (equipo != null)
                {
                    equipo.Stock += 1;

                    // si tu lógica usa Status 2 = agotado, reactivarlo si ahora hay stock
                    if (equipo.Stock > 0 && equipo.Status == 2)
                        equipo.Status = 1;
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

                    // Join opcional con Empleado (técnico)
                    join emp in contexto.Empleado
                        on m.IdEmpleado equals emp.IdEmpleado into empJoin
                    from emp in empJoin.DefaultIfEmpty()

                        // Join opcional con Orden
                    join ord in contexto.RentalOrders
                        on m.OrderId equals ord.OrderId into ordJoin
                    from ord in ordJoin.DefaultIfEmpty()

                        // Join opcional con Cliente
                    join cli in contexto.Clients
                        on ord.ClientId equals cli.ClientId into cliJoin
                    from cli in cliJoin.DefaultIfEmpty()

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
                        FinalizadoPor = m.FinalizadoPor,

                        // 🔹 Ya tenías estos:
                        OrderId = m.OrderId,
                        ClientName = cli != null
                            ? ((cli.FirstName ?? "") + " " + (cli.LastName ?? "")).Trim()
                            : null,

                        // 🔹 NUEVO: necesitamos el Id del cliente para el modal
                        ClientId = cli != null
                            ? cli.ClientId.ToString()
                            : null
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


        public ActionResult Faltantes(
            string cliente,
            string equipo,
            int? cantidadMin,
            int? estado
        )
        {
            using (var contexto = new Contexto())
            {
                // 1) Mantenimientos de tipo faltante (ya tienen OrderId y EquipmentId)
                var mantenimientos = (from m in contexto.Maintenance
                                      where m.MaintenanceType == 4    // 4 = faltante / no devuelto
                                      join eq in contexto.Equipment
                                           on m.EquipmentId equals eq.EquipmentId
                                      join ro in contexto.RentalOrders
                                           on m.OrderId equals ro.OrderId
                                      join cl in contexto.Clients
                                           on ro.ClientId equals cl.ClientId
                                      select new
                                      {
                                          m.MaintenanceId,
                                          m.OrderId,
                                          m.EquipmentId,
                                          EquipmentName = eq.EquipmentName,
                                          Cliente = cl.FirstName + " " + cl.LastName,
                                          m.MaintenanceStatus,
                                          m.Cost,
                                          m.StartDate,
                                          m.EndDate,
                                          ImageUrl = eq.ImageUrl
                                      })
                                      .ToList(); // de aquí en adelante estamos en memoria

                // 2) Cantidades de faltantes por Orden + Equipo (solo ReturnDetails con IsReturned = false)
                var cantidades = contexto.ReturnDetails
                    .Where(rd => !rd.IsReturned)
                    .GroupBy(rd => new { rd.OrderId, rd.EquipmentId })
                    .Select(g => new
                    {
                        g.Key.OrderId,
                        g.Key.EquipmentId,
                        Cantidad = g.Count()
                    })
                    .ToList();

                // 3) Diccionario (OrderId, EquipmentId) -> Cantidad
                var mapaCantidades = cantidades
                    .ToDictionary(
                        x => Tuple.Create(x.OrderId, x.EquipmentId),
                        x => x.Cantidad
                    );

                // 4) Armamos el DTO final
                var faltantes = mantenimientos
                    .Select(m =>
                    {
                        int cantidad = 0;

                        if (m.OrderId.HasValue)
                        {
                            mapaCantidades.TryGetValue(
                                Tuple.Create(m.OrderId.Value, m.EquipmentId),
                                out cantidad);
                        }

                        return new MaintenanceDto
                        {
                            MaintenanceId = m.MaintenanceId,
                            OrderId = m.OrderId,
                            EquipmentId = m.EquipmentId,
                            EquipmentName = m.EquipmentName,
                            ClientName = m.Cliente,
                            Cantidad = cantidad,
                            MaintenanceStatus = m.MaintenanceStatus,
                            Cost = m.Cost,
                            StartDate = m.StartDate,
                            EndDate = m.EndDate,
                            ImageUrl = m.ImageUrl
                        };
                    })
                    .Where(f => f.Cantidad > 0)
                    .OrderByDescending(f => f.OrderId)
                    .ThenBy(f => f.EquipmentName)
                    .ToList();

                // 🔍 Filtros en memoria
                if (!string.IsNullOrWhiteSpace(cliente))
                {
                    var term = cliente.Trim().ToLower();
                    faltantes = faltantes
                        .Where(f => (f.ClientName ?? "").ToLower().Contains(term))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(equipo))
                {
                    var term = equipo.Trim().ToLower();
                    faltantes = faltantes
                        .Where(f => (f.EquipmentName ?? "").ToLower().Contains(term))
                        .ToList();
                }

                if (cantidadMin.HasValue)
                {
                    faltantes = faltantes
                        .Where(f => f.Cantidad >= cantidadMin.Value)
                        .ToList();
                }

                if (estado.HasValue)
                {
                    faltantes = faltantes
                        .Where(f => f.MaintenanceStatus == estado.Value)
                        .ToList();
                }

                // 🎯 ViewBags para mantener los filtros en la vista
                ViewBag.FiltroCliente = cliente;
                ViewBag.FiltroEquipo = equipo;
                ViewBag.FiltroCantidadMin = cantidadMin;
                ViewBag.FiltroEstado = estado;

                // 🟢 Combo de estados (esto es lo que te faltaba y daba la excepción)
                ViewBag.Estados = new[]
                {
            new SelectListItem { Text = "Todos",      Value = "",  Selected = !estado.HasValue },
            new SelectListItem { Text = "Pendiente",  Value = "0", Selected = estado == 0 },
            new SelectListItem { Text = "Finalizado", Value = "1", Selected = estado == 1 }
        };

                return View("Faltantes", faltantes);
            }
        }






        // GET: ReturnDetails/EditFaltante/5
        public ActionResult EditFaltante(int id)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance.Find(id);
                if (mantenimiento == null || mantenimiento.MaintenanceType != 4)
                    return HttpNotFound();

                var equipo = contexto.Equipment.FirstOrDefault(e => e.EquipmentId == mantenimiento.EquipmentId);

                // Buscamos info de orden/cliente y cantidad faltante
                var datosOrden = (from rd in contexto.ReturnDetails
                                  join ro in contexto.RentalOrders on rd.OrderId equals ro.OrderId
                                  join cl in contexto.Clients on ro.ClientId equals cl.ClientId
                                  where rd.IsReturned == false
                                        && rd.EquipmentId == mantenimiento.EquipmentId
                                  select new
                                  {
                                      rd.OrderId,
                                      Cliente = cl.FirstName + " " + cl.LastName
                                  })
                                  .ToList();

                int? orderId = datosOrden.FirstOrDefault()?.OrderId;
                string clientName = datosOrden.FirstOrDefault()?.Cliente;
                int cantidadFaltante = datosOrden.Count; // cuántos ReturnDetails no devueltos hay para ese equipo

                var modelo = new MaintenanceDto
                {
                    MaintenanceId = mantenimiento.MaintenanceId,
                    StartDate = mantenimiento.StartDate,
                    EndDate = mantenimiento.EndDate,
                    MaintenanceType = mantenimiento.MaintenanceType,
                    MaintenanceStatus = mantenimiento.MaintenanceStatus,
                    EquipmentId = mantenimiento.EquipmentId,
                    EquipmentName = equipo?.EquipmentName,
                    Comments = mantenimiento.Comments,
                    Cost = mantenimiento.Cost,
                    IdEmpleado = mantenimiento.IdEmpleado,
                    Cantidad = cantidadFaltante,
                    OrderId = orderId,
                    ClientName = clientName
                };

                return View("EditFaltante", modelo);
            }
        }


        // POST: ReturnDetails/EditFaltante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFaltante(MaintenanceDto model)
        {
            if (!ModelState.IsValid)
            {
                // Si algo falla en validación volvemos a mostrar la vista con los datos
                return View("EditFaltante", model);
            }

            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance.Find(model.MaintenanceId);
                if (mantenimiento == null || mantenimiento.MaintenanceType != 4)
                    return HttpNotFound();

                mantenimiento.Comments = model.Comments;
                mantenimiento.Cost = model.Cost;
                mantenimiento.MaintenanceStatus = model.MaintenanceStatus;

                // Si lo marcamos como finalizado y no tiene EndDate, la seteamos
                if (mantenimiento.MaintenanceStatus == 1 && !mantenimiento.EndDate.HasValue)
                {
                    mantenimiento.EndDate = DateTime.Now;
                }

                contexto.SaveChanges();
            }

            TempData["Success"] = "Registro de equipo faltante actualizado correctamente.";
            return RedirectToAction("Faltantes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizarFaltante(int id)
        {
            using (var contexto = new Contexto())
            {
                var mantenimiento = contexto.Maintenance.Find(id);
                if (mantenimiento == null || mantenimiento.MaintenanceType != 4)
                    return HttpNotFound();

                // Marcar como finalizado
                mantenimiento.MaintenanceStatus = 1; // 1 = Finalizado

                if (!mantenimiento.EndDate.HasValue)
                {
                    mantenimiento.EndDate = DateTime.Now;
                }

                // Opcional: dejar un rastro en comentarios
                if (string.IsNullOrEmpty(mantenimiento.Comments))
                {
                    mantenimiento.Comments = "Faltante marcado como finalizado.";
                }
                else if (!mantenimiento.Comments.Contains("finalizado"))
                {
                    mantenimiento.Comments += $" | Finalizado el {DateTime.Now:dd/MM/yyyy}";
                }

                contexto.SaveChanges();
            }

            TempData["Success"] = "El equipo faltante se marcó como finalizado correctamente.";
            return RedirectToAction("Faltantes");
        }

        public ActionResult DetallesFaltante(int id)
        {
            using (var contexto = new Contexto())
            {
                var m = contexto.Maintenance
                    .Include("Equipment")
                    .Include("RentalOrder.Client")
                    .FirstOrDefault(x => x.MaintenanceId == id && x.MaintenanceType == 4);

                if (m == null)
                    return HttpNotFound();

                // Referencias seguras
                var orden = m.RentalOrder;
                var cliente = orden != null ? orden.Client : null;

                // Cantidad faltante SOLO de esta orden y este equipo
                int cantidadFaltante = contexto.ReturnDetails
                    .Where(rd =>
                        rd.OrderId == m.OrderId &&
                        rd.EquipmentId == m.EquipmentId &&
                        rd.IsReturned == false)
                    .Count();

                // Nombre completo del cliente (si existe)
                string clienteNombre = cliente != null
                    ? string.Format("{0} {1}", cliente.FirstName ?? "", cliente.LastName ?? "").Trim()
                    : "Desconocido";

                var dto = new MaintenanceDto
                {
                    MaintenanceId = m.MaintenanceId,
                    OrderId = m.OrderId,
                    EquipmentId = m.EquipmentId,
                    EquipmentName = m.Equipment.EquipmentName,
                    ClientName = clienteNombre,
                    ClientId = cliente != null ? cliente.ClientId.ToString() : null, 
                    Cantidad = cantidadFaltante,
                    MaintenanceStatus = m.MaintenanceStatus,
                    Cost = m.Cost,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Comments = m.Comments
                };

                if (orden != null)
                {
                    ViewBag.OrderDate = orden.StartDate;
                }

                return View(dto);
            }
        }




    }
}
