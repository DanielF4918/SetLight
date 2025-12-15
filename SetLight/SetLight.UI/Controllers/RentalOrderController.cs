using System;
using System.Collections.Generic;
using System.Data.Entity; // Para Include
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.CrearRentalOrder;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.Abstracciones.ViewModels;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Clientes.ObtenerClPorID;
using SetLight.AccesoADatos.Equipment.ObtenerEqPorID;
using SetLight.AccesoADatos.Modelos;
using SetLight.AccesoADatos.rentalorder.EditRentalOrder;
using SetLight.AccesoADatos.rentalorder.ObtenerROPorId;
using SetLight.AccesoADatos.RentalOrder;
using SetLight.LogicaDeNegocio.Services;
using PagedList;
using X.PagedList;

namespace SetLight.UI.Controllers
{
    [Authorize(Roles = "Administrador,Colaborador")]
    public class RentalOrderController : Controller
    {
        private Contexto _contexto;
        private ObtenerClPorIDAD _obtenerClPorID;
        private ListarRentalOrderAD _listarOrdenesAD;
        private CrearRentalOrderAD _crearOrdenAD;
        private EditRentalOrderAD _editarOrdenAD;
        private ObtenerROPorIdAD _obtenerROPorIdAD;

        public RentalOrderController()
        {
            _contexto = new Contexto();
            _obtenerClPorID = new ObtenerClPorIDAD();
            _listarOrdenesAD = new ListarRentalOrderAD();
            _crearOrdenAD = new CrearRentalOrderAD();
            _editarOrdenAD = new EditRentalOrderAD();
            _obtenerROPorIdAD = new ObtenerROPorIdAD();
        }

        // =======================
        // HISTORIAL POR CLIENTE
        // =======================
        public ActionResult History(int? clientId, int? page, DateTime? desde, DateTime? hasta)
        {
            // Si viene sin clientId, redirigimos al listado de clientes
            if (!clientId.HasValue)
            {
                return RedirectToAction("ListarClient", "Client");
            }

            var id = clientId.Value;

            ClientDto cliente = _obtenerClPorID.Obtener(id);
            if (cliente == null)
                return HttpNotFound("Cliente no encontrado");

            var historial = (from orden in _contexto.RentalOrders
                             where orden.ClientId == id
                             select new RentalOrderDto
                             {
                                 OrderId = orden.OrderId,
                                 OrderDate = orden.OrderDate,
                                 StartDate = orden.StartDate,
                                 EndDate = orden.EndDate,
                                 StatusOrder = orden.StatusOrder,
                                 ClientId = orden.ClientId,
                                 ClientName = cliente.FirstName + " " + cliente.LastName,
                                 RutaComprobante = orden.RutaComprobante,
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
                             });

            // filtros opcionales por fecha de orden
            if (desde.HasValue) historial = historial.Where(o => o.OrderDate >= desde.Value);
            if (hasta.HasValue) historial = historial.Where(o => o.OrderDate <= hasta.Value);

            int pageSize = 7;
            int pageNumber = page ?? 1;
            var historialPaginado = historial
                .OrderByDescending(x => x.OrderId)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.ClientName = cliente.FirstName + " " + cliente.LastName;
            ViewBag.FiltroDesde = desde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = hasta?.ToString("yyyy-MM-dd");
            ViewBag.ClientId = id;

            return View(historialPaginado);
        }

        // =======================
        // LISTADO GENERAL
        // =======================
        // GET: /RentalOrder
        public ActionResult Index(
            int? page,
            int? orderId,
            int? estado,            // 1=Activa, 2=Completada, 3=Cancelada
            string cliente,         // parte del nombre
            int? empleadoId,        // opcional
            DateTime? desde,        // StartDate >=
            DateTime? hasta         // EndDate   <=
        )
        {
            var q = from orden in _contexto.RentalOrders
                    join clienteTbl in _contexto.Clients on orden.ClientId equals clienteTbl.ClientId
                    join empleado in _contexto.Empleado on orden.EmpleadoId equals empleado.IdEmpleado into empJoin
                    from empleado in empJoin.DefaultIfEmpty()
                    select new RentalOrderDto
                    {
                        OrderId = orden.OrderId,
                        OrderDate = orden.OrderDate,
                        StartDate = orden.StartDate,
                        EndDate = orden.EndDate,
                        StatusOrder = orden.StatusOrder,
                        ClientId = orden.ClientId,
                        ClientName = clienteTbl.FirstName + " " + clienteTbl.LastName,
                        EmpleadoId = orden.EmpleadoId,
                        EmpleadoNombreCompleto = empleado != null
                            ? empleado.Nombre + " " + empleado.Apellido
                            : "No asignado",
                        RutaComprobante = orden.RutaComprobante,
                        Details = (from detalle in _contexto.OrderDetails
                                   join equipo in _contexto.Equipment on detalle.EquipmentId equals equipo.EquipmentId
                                   where detalle.OrderId == orden.OrderId
                                   select new OrderDetailDto
                                   {
                                       EquipmentName = equipo.EquipmentName,
                                       Brand = equipo.Brand,
                                       Model = equipo.Model,
                                       RentalValue = equipo.RentalValue,
                                       Quantity = detalle.Quantity
                                   }).ToList()
                    };

            // Filtros condicionales
            if (orderId.HasValue) q = q.Where(o => o.OrderId == orderId.Value);
            if (estado.HasValue) q = q.Where(o => o.StatusOrder == estado.Value);
            if (!string.IsNullOrWhiteSpace(cliente))
            {
                var term = cliente.Trim().ToLower();
                q = q.Where(o => (o.ClientName ?? "").ToLower().Contains(term));
            }
            if (empleadoId.HasValue) q = q.Where(o => o.EmpleadoId == empleadoId.Value);
            if (desde.HasValue) q = q.Where(o => o.StartDate >= desde.Value);
            if (hasta.HasValue) q = q.Where(o => o.EndDate <= hasta.Value);

            // Orden descendente por ID
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var ordenesPaged = q.OrderByDescending(o => o.OrderId)
                     .ToPagedList(pageNumber, pageSize);

            // Para valores en la vista
            ViewBag.FiltroOrderId = orderId;
            ViewBag.FiltroEstado = estado;
            ViewBag.FiltroCliente = cliente;
            ViewBag.FiltroEmpleadoId = empleadoId;
            ViewBag.FiltroDesde = desde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = hasta?.ToString("yyyy-MM-dd");

            // Dropdown de empleados
            ViewBag.Empleados = _contexto.Empleado
                .Select(e => new SelectListItem
                {
                    Value = e.IdEmpleado.ToString(),
                    Text = e.Nombre + " " + e.Apellido
                })
                .OrderBy(x => x.Text)
                .ToList();

            return View(ordenesPaged);
        }

        // =======================
        // CREATE GET
        // =======================
        public ActionResult Create()
        {
            // Solo clientes ACTIVOS (Status == 1)
            var clientes = _contexto.Clients
                .Where(c => c.Status == 1)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Select(c => new ClientDto
                {
                    ClientId = c.ClientId,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                })
                .ToList();

            var equipos = _contexto.Equipment
                .Where(e => e.Status == 1 && e.Stock > 0)
                .Select(e => new OrderDetailDto
                {
                    EquipmentId = e.EquipmentId,
                    EquipmentName = e.EquipmentName,
                    Brand = e.Brand,
                    Model = e.Model,
                    RentalValue = e.RentalValue,
                    Quantity = 0,
                    Stock = e.Stock
                })
                .ToList();

            var model = new CrearRentalOrderViewModel
            {
                Clientes = clientes,
                EquiposDisponibles = equipos,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                StatusOrder = 1
            };

            return View(model);
        }


        // ======================= 
        // CREATE POST
        // =======================
        [HttpPost]
        public async Task<ActionResult> Create(CrearRentalOrderViewModel model)
        {
            // =======================
            // Helper: recargar combos y rehidratar equipos
            // =======================
            void RecargarCombos(CrearRentalOrderViewModel m, List<OrderDetailDto> equiposSel = null)
            {
                m.Clientes = _contexto.Clients
                    .Where(c => c.Status == 1)
                    .Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName
                    }).ToList();

                m.EquiposDisponibles = _contexto.Equipment
                    .Where(e => e.Status == 1 && e.Stock > 0)
                    .Select(e => new OrderDetailDto
                    {
                        EquipmentId = e.EquipmentId,
                        EquipmentName = e.EquipmentName,
                        Brand = e.Brand,
                        Model = e.Model,
                        RentalValue = e.RentalValue,
                        Quantity = 0,
                        Stock = e.Stock
                    }).ToList();

                // 🔁 Rehidratar cantidades seleccionadas
                if (equiposSel != null && equiposSel.Any())
                {
                    var map = equiposSel.ToDictionary(x => x.EquipmentId, x => x.Quantity);

                    foreach (var eq in m.EquiposDisponibles)
                    {
                        if (map.TryGetValue(eq.EquipmentId, out var qty))
                            eq.Quantity = qty;
                    }

                    m.EquiposSeleccionados = equiposSel;
                }
            }

            // =======================
            // Validación básica del modelo
            // =======================
            if (!ModelState.IsValid)
            {
                RecargarCombos(model, model.EquiposSeleccionados);
                return View(model);
            }

            var equiposSeleccionados = model.EquiposSeleccionados?
                .Where(e => e.Quantity > 0)
                .ToList();

            if (equiposSeleccionados == null || !equiposSeleccionados.Any())
            {
                ModelState.AddModelError("", "Debe ingresar la cantidad de al menos un equipo.");
                RecargarCombos(model, model.EquiposSeleccionados);
                return View(model);
            }

            // =======================
            // Validación crítica: Cliente activo
            // =======================
            var clienteDb = _contexto.Clients.FirstOrDefault(c => c.ClientId == model.ClientId);
            if (clienteDb == null || clienteDb.Status != 1)
            {
                ModelState.AddModelError("",
                    "No se puede finalizar: el cliente está inactivo o fue desactivado. Refresque y seleccione otro cliente.");
                RecargarCombos(model, equiposSeleccionados);
                return View(model);
            }

            // =======================
            // Validación crítica: Empleado activo
            // =======================
            string correoUsuario = User.Identity?.Name ?? "";
            var empleadoDb = _contexto.Empleado
                .FirstOrDefault(e => e.CorreoElectronico == correoUsuario);

            if (empleadoDb == null)
            {
                ModelState.AddModelError("",
                    "No se pudo identificar el empleado autenticado. Inicie sesión nuevamente.");
                RecargarCombos(model, equiposSeleccionados);
                return View(model);
            }

            if (!empleadoDb.Estado)
            {
                ModelState.AddModelError("",
                    "No se puede finalizar: su usuario fue desactivado mientras realizaba la orden.");
                RecargarCombos(model, equiposSeleccionados);
                return View(model);
            }

            // =======================
            // Construcción de la orden
            // =======================
            var nuevaOrden = new RentalOrderDto
            {
                ClientId = model.ClientId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StatusOrder = model.StatusOrder,
                EmpleadoId = empleadoDb.IdEmpleado,
                DescuentoManual = model.DescuentoManual,
                Details = equiposSeleccionados.Select(e => new OrderDetailDto
                {
                    EquipmentId = e.EquipmentId,
                    EquipmentName = e.EquipmentName,
                    Brand = e.Brand,
                    Model = e.Model,
                    Quantity = e.Quantity,
                    RentalValue = e.RentalValue
                }).ToList()
            };

            // =======================
            // Guardado
            // =======================
            try
            {
                var crearLN = new CrearRentalOrderLN(_crearOrdenAD);
                await crearLN.Guardar(nuevaOrden);

                var ordenGuardada = _contexto.RentalOrders
                    .OrderByDescending(o => o.OrderId)
                    .FirstOrDefault(o => o.ClientId == model.ClientId && o.StartDate == model.StartDate);

                if (ordenGuardada != null && (ordenGuardada.StatusOrder == 1 || ordenGuardada.StatusOrder == 2))
                {
                    var ordenParaPDF = new RentalOrderDto
                    {
                        OrderId = ordenGuardada.OrderId,
                        OrderDate = ordenGuardada.OrderDate,
                        StartDate = ordenGuardada.StartDate,
                        EndDate = ordenGuardada.EndDate,
                        ClientName = clienteDb.FirstName + " " + clienteDb.LastName,
                        Details = equiposSeleccionados
                    };

                    byte[] pdfBytes = ComprobantePdfService.GenerarEnMemoria(ordenParaPDF);
                    string fileName = $"Orden_{ordenParaPDF.OrderId}.pdf";
                    ordenGuardada.RutaComprobante = fileName;
                    await _contexto.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                RecargarCombos(model, equiposSeleccionados);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty,
                    "Ocurrió un error al guardar la orden. Por favor, intente de nuevo.");

                RecargarCombos(model, equiposSeleccionados);
                return View(model);
            }
        }



        // =======================
        // EDIT GET
        // =======================
        public ActionResult Edit(int id)
        {
            var orden = _contexto.RentalOrders.FirstOrDefault(o => o.OrderId == id);
            if (orden == null)
                return HttpNotFound();

            // Detalles seleccionados (equipos ya en la orden)
            var detalles = (from detalle in _contexto.OrderDetails
                            where detalle.OrderId == id && detalle.Quantity > 0
                            join equipo in _contexto.Equipment
                                on detalle.EquipmentId equals equipo.EquipmentId
                            select new OrderDetailDto
                            {
                                EquipmentId = equipo.EquipmentId,
                                EquipmentName = equipo.EquipmentName,
                                Brand = equipo.Brand,
                                Model = equipo.Model,
                                RentalValue = equipo.RentalValue,
                                Quantity = detalle.Quantity,
                                Stock = equipo.Stock
                            }).ToList();

            var cantidadesPorEquipo = detalles.ToDictionary(d => d.EquipmentId, d => d.Quantity);
            var idsSeleccionados = cantidadesPorEquipo.Keys.ToList();

            // Equipos: activos o que ya están en la orden (aunque estén inactivos)
            var equiposBase = _contexto.Equipment
                .Where(e => e.Status == 1 || idsSeleccionados.Contains(e.EquipmentId))
                .ToList();

            var equiposParaModal = equiposBase
                .Select(e =>
                {
                    cantidadesPorEquipo.TryGetValue(e.EquipmentId, out int qty);
                    return new OrderDetailDto
                    {
                        EquipmentId = e.EquipmentId,
                        EquipmentName = e.EquipmentName,
                        Brand = e.Brand,
                        Model = e.Model,
                        RentalValue = e.RentalValue,
                        Stock = e.Stock,
                        Quantity = qty
                    };
                })
                .ToList();

            // ✅ Clientes para modal:
            // Solo activos, PERO incluye el cliente actual aunque esté inactivo
            var clientes = _contexto.Clients
                .Where(c => c.Status == 1 || c.ClientId == orden.ClientId)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Select(c => new ClientDto
                {
                    ClientId = c.ClientId,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                })
                .ToList();

            // (Opcional pero útil para la vista: nombre del cliente seleccionado)
            var clienteActual = clientes.FirstOrDefault(c => c.ClientId == orden.ClientId);
            string nombreClienteActual = clienteActual != null
                ? $"{clienteActual.FirstName} {clienteActual.LastName}"
                : "Cliente no disponible";

            var viewModel = new CrearRentalOrderViewModel
            {
                OrderId = orden.OrderId,
                ClientId = orden.ClientId,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                StatusOrder = orden.StatusOrder,
                DescuentoManual = orden.DescuentoManual,

                EquiposSeleccionados = detalles,
                EquiposDisponibles = equiposParaModal,

                Clientes = clientes,

                // Si tu VM tiene algún campo para mostrar nombre (si no, lo ponemos en ViewBag)
                // ClientName = nombreClienteActual
            };

            ViewBag.ClientNameSeleccionado = nombreClienteActual;

            return View("Edit", viewModel);
        }


        // =======================
        // EDIT POST
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CrearRentalOrderViewModel model)
        {
            var equiposSeleccionados = model.EquiposSeleccionados?
                .Where(e => e.Quantity > 0)
                .ToList() ?? new List<OrderDetailDto>();

            if (!ModelState.IsValid || !equiposSeleccionados.Any())
            {
                if (!equiposSeleccionados.Any())
                {
                    ModelState.AddModelError("", "Debe ingresar la cantidad de al menos un equipo.");
                }

                model.Clientes = _contexto.Clients.Select(c => new ClientDto
                {
                    ClientId = c.ClientId,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                }).ToList();

                model.EquiposDisponibles = _contexto.Equipment
                    .Where(e => e.Status == 1 && e.Stock > 0)
                    .Select(e => new OrderDetailDto
                    {
                        EquipmentId = e.EquipmentId,
                        EquipmentName = e.EquipmentName,
                        Brand = e.Brand,
                        Model = e.Model,
                        RentalValue = e.RentalValue,
                        Stock = e.Stock
                    }).ToList();

                model.EquiposSeleccionados = equiposSeleccionados;

                return View(model);
            }

            using (var transaction = _contexto.Database.BeginTransaction())
            {
                try
                {
                    var orden = await _contexto.RentalOrders.FindAsync(id);
                    if (orden == null)
                        return HttpNotFound();

                    orden.ClientId = model.ClientId;
                    orden.StartDate = model.StartDate;
                    orden.EndDate = model.EndDate;
                    orden.OrderDate = DateTime.Now;
                    orden.DescuentoManual = model.DescuentoManual;

                    var detallesAnteriores = _contexto.OrderDetails.Where(d => d.OrderId == id).ToList();

                    foreach (var detalle in detallesAnteriores)
                    {
                        var equipo = await _contexto.Equipment.FindAsync(detalle.EquipmentId);
                        if (equipo != null)
                        {
                            equipo.Stock += detalle.Quantity;

                            if (equipo.Stock > 0 && equipo.Status == 2)
                                equipo.Status = 1;
                        }
                    }

                    _contexto.OrderDetails.RemoveRange(detallesAnteriores);
                    await _contexto.SaveChangesAsync();

                    foreach (var item in equiposSeleccionados)
                    {
                        var equipo = await _contexto.Equipment.FindAsync(item.EquipmentId);
                        if (equipo != null)
                        {
                            if (equipo.Stock < item.Quantity)
                            {
                                throw new InvalidOperationException(
                                    $"No hay suficiente stock para el equipo: {equipo.EquipmentName}. " +
                                    $"Disponibles: {equipo.Stock}, seleccionados: {item.Quantity}."
                                );
                            }

                            _contexto.OrderDetails.Add(new OrderDetailDA
                            {
                                OrderId = id,
                                EquipmentId = item.EquipmentId,
                                Quantity = item.Quantity
                            });

                            equipo.Stock -= item.Quantity;

                            if (equipo.Stock <= 0)
                            {
                                equipo.Stock = 0;
                                equipo.Status = 2;
                            }
                        }
                    }

                    await _contexto.SaveChangesAsync();
                    transaction.Commit();

                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    transaction.Rollback();

                    ModelState.AddModelError(string.Empty, ex.Message);

                    model.Clientes = _contexto.Clients.Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName
                    }).ToList();

                    model.EquiposDisponibles = _contexto.Equipment
                        .Where(e => e.Status == 1)
                        .Select(e => new OrderDetailDto
                        {
                            EquipmentId = e.EquipmentId,
                            EquipmentName = e.EquipmentName,
                            Brand = e.Brand,
                            Model = e.Model,
                            RentalValue = e.RentalValue,
                            Stock = e.Stock
                        }).ToList();

                    model.EquiposSeleccionados = equiposSeleccionados;

                    return View(model);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Error al guardar los cambios.");

                    model.Clientes = _contexto.Clients.Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName
                    }).ToList();

                    model.EquiposDisponibles = _contexto.Equipment
                        .Where(e => e.Status == 1 && e.Stock > 0)
                        .Select(e => new OrderDetailDto
                        {
                            EquipmentId = e.EquipmentId,
                            EquipmentName = e.EquipmentName,
                            Brand = e.Brand,
                            Model = e.Model,
                            RentalValue = e.RentalValue,
                            Stock = e.Stock
                        }).ToList();

                    model.EquiposSeleccionados = equiposSeleccionados;

                    return View(model);
                }
            }
        }

        // =======================
        // CANCELAR ORDEN
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancelar(int id)
        {
            using (var transaction = _contexto.Database.BeginTransaction())
            {
                try
                {
                    var orden = await _contexto.RentalOrders.FindAsync(id);
                    if (orden == null)
                        return HttpNotFound();

                    if (orden.StatusOrder != 1)
                        return RedirectToAction("Index");

                    var detalles = _contexto.OrderDetails
                        .Where(d => d.OrderId == id)
                        .ToList();

                    foreach (var detalle in detalles)
                    {
                        var equipo = await _contexto.Equipment.FindAsync(detalle.EquipmentId);
                        if (equipo != null)
                        {
                            equipo.Stock += detalle.Quantity;

                            if (equipo.Stock > 0 && equipo.Status == 2)
                                equipo.Status = 1;
                        }
                    }

                    orden.StatusOrder = 3;

                    await _contexto.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }

            return RedirectToAction("Index");
        }

        // =======================
        // VER COMPROBANTE
        // =======================
        public ActionResult VerComprobante(int id)
        {
            var orden = _contexto.RentalOrders.Find(id);
            if (orden == null)
                return HttpNotFound("Orden no encontrada");

            var cliente = _contexto.Clients.FirstOrDefault(c => c.ClientId == orden.ClientId);
            if (cliente == null)
                return HttpNotFound("Cliente no encontrado");

            var detalles = (from detalle in _contexto.OrderDetails
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
                            }).ToList();

            var dias = (orden.EndDate - orden.StartDate).Days + 1;
            if (dias < 1) dias = 1;

            decimal subtotal = 0m;
            foreach (var d in detalles)
            {
                subtotal += d.RentalValue * d.Quantity * dias;
            }

            var iva = Math.Round(subtotal * 0.13m, 2);
            var totalBruto = subtotal + iva;

            var descuentoPct = orden.DescuentoManual ?? 0m;
            var montoDescuento = Math.Round(totalBruto * (descuentoPct / 100m), 2);
            var total = totalBruto - montoDescuento;

            var dto = new RentalOrderDto
            {
                OrderId = orden.OrderId,
                OrderDate = orden.OrderDate,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                ClientName = $"{cliente.FirstName} {cliente.LastName}",
                Details = detalles,
                DescuentoManual = orden.DescuentoManual,
                CantidadDias = dias,
                Subtotal = subtotal,
                Iva = iva,
                Total = total
            };

            byte[] pdfBytes = ComprobantePdfService.GenerarEnMemoria(dto);
            return File(pdfBytes, "application/pdf", $"Comprobante_Orden_{dto.OrderId}.pdf");
        }
    }
}
