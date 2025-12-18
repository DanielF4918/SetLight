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
    [Authorize(Roles = "Administrador,Colaborador,Tecnico")]
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
                                                RentalValue = detalle.UnitRentalPrice,
                                                Quantity = detalle.Quantity
                                            }).ToList()
                             });

            // filtros opcionales por fecha de orden
            if (desde.HasValue) historial = historial.Where(o => o.OrderDate >= desde.Value);
            if (hasta.HasValue) historial = historial.Where(o => o.OrderDate <= hasta.Value);

            int pageSize = 12;
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
     int? estado,
     string cliente,
     int? empleadoId,
     DateTime? desde,
     DateTime? hasta
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

                        // ✅ NUEVO: campos de entrega/costos
                        IsDelivery = orden.IsDelivery,
                        DeliveryAddress = orden.DeliveryAddress,
                        TransportCost = orden.TransportCost,
                        DescuentoManual = orden.DescuentoManual,

                        Details = (from detalle in _contexto.OrderDetails
                                   join equipo in _contexto.Equipment on detalle.EquipmentId equals equipo.EquipmentId
                                   where detalle.OrderId == orden.OrderId
                                   select new OrderDetailDto
                                   {
                                       EquipmentId = detalle.EquipmentId,
                                       EquipmentName = equipo.EquipmentName,
                                       Brand = equipo.Brand,
                                       Model = equipo.Model,
                                       RentalValue = detalle.UnitRentalPrice, // ✅ snapshot
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

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var ordenesPaged = q.OrderByDescending(o => o.OrderId)
                     .ToPagedList(pageNumber, pageSize);

            ViewBag.FiltroOrderId = orderId;
            ViewBag.FiltroEstado = estado;
            ViewBag.FiltroCliente = cliente;
            ViewBag.FiltroEmpleadoId = empleadoId;
            ViewBag.FiltroDesde = desde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = hasta?.ToString("yyyy-MM-dd");

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
                StatusOrder = 1,

                IsDelivery = false,
                DeliveryAddress = null,
                TransportCost = 0m
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
            // ✅ Validaciones de entrega (Misión 2)
            // =======================
            if (model.IsDelivery)
            {
                if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                {
                    ModelState.AddModelError(nameof(model.DeliveryAddress), "Debe ingresar la dirección de entrega.");
                    RecargarCombos(model, equiposSeleccionados);
                    return View(model);
                }

                if (model.TransportCost < 0)
                {
                    ModelState.AddModelError(nameof(model.TransportCost), "El costo de transporte no puede ser negativo.");
                    RecargarCombos(model, equiposSeleccionados);
                    return View(model);
                }

                model.DeliveryAddress = model.DeliveryAddress.Trim();
            }
            else
            {
                // Si NO hay entrega, forzamos valores neutros para evitar basura en BD
                model.DeliveryAddress = null;
                model.TransportCost = 0m;
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
            // ✅ Congelar precios desde BD (snapshot)
            // =======================
            var idsEquipos = equiposSeleccionados
                .Select(x => x.EquipmentId)
                .Distinct()
                .ToList();

            // Traemos los equipos desde BD para tomar el RentalValue real actual (no confiar en UI)
            var equiposDb = _contexto.Equipment
                .Where(x => idsEquipos.Contains(x.EquipmentId))
                .Select(x => new
                {
                    x.EquipmentId,
                    x.Status,
                    x.EquipmentName,
                    x.Brand,
                    x.Model,
                    x.RentalValue
                })
                .ToList();

            // Validación: que existan todos y estén activos
            if (equiposDb.Count != idsEquipos.Count || equiposDb.Any(x => x.Status != 1))
            {
                ModelState.AddModelError("",
                    "No se puede finalizar: uno o más equipos seleccionados ya no existen o fueron desactivados. Refresque la lista e intente nuevamente.");
                RecargarCombos(model, equiposSeleccionados);
                return View(model);
            }

            var mapEquiposDb = equiposDb.ToDictionary(x => x.EquipmentId, x => x);

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

                // ✅ Misión 2
                IsDelivery = model.IsDelivery,
                DeliveryAddress = model.DeliveryAddress,
                TransportCost = model.TransportCost,

                // ✅ Details con RentalValue tomado desde BD (precio pactado / snapshot)
                Details = equiposSeleccionados.Select(e =>
                {
                    var eqDb = mapEquiposDb[e.EquipmentId];

                    return new OrderDetailDto
                    {
                        EquipmentId = e.EquipmentId,
                        EquipmentName = eqDb.EquipmentName,
                        Brand = eqDb.Brand,
                        Model = eqDb.Model,
                        Quantity = e.Quantity,

                        // Este es el precio que debe guardarse luego en OrderDetails.UnitRentalPrice
                        RentalValue = eqDb.RentalValue
                    };
                }).ToList()
            };

            // =======================
            // Guardado
            // =======================
            try
            {
                var crearLN = new CrearRentalOrderLN(_crearOrdenAD);
                await crearLN.Guardar(nuevaOrden);

                // Buscar la última orden creada para ese cliente y fechas (más robusto)
                var ordenGuardada = _contexto.RentalOrders
                    .Where(o =>
                        o.ClientId == model.ClientId &&
                        o.StartDate == model.StartDate &&
                        o.EndDate == model.EndDate)
                    .OrderByDescending(o => o.OrderId)
                    .FirstOrDefault();

                if (ordenGuardada != null && (ordenGuardada.StatusOrder == 1 || ordenGuardada.StatusOrder == 2))
                {
                    var ordenParaPDF = new RentalOrderDto
                    {
                        OrderId = ordenGuardada.OrderId,
                        OrderDate = ordenGuardada.OrderDate,
                        StartDate = ordenGuardada.StartDate,
                        EndDate = ordenGuardada.EndDate,
                        ClientName = clienteDb.FirstName + " " + clienteDb.LastName,

                        // ✅ usar los details con precio congelado (no los del form)
                        Details = nuevaOrden.Details,

                        // ✅ Misión 2
                        IsDelivery = ordenGuardada.IsDelivery,
                        DeliveryAddress = ordenGuardada.DeliveryAddress,
                        TransportCost = ordenGuardada.TransportCost,

                        // Si tu PDF ocupa el descuento, lo incluimos también
                        DescuentoManual = ordenGuardada.DescuentoManual
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

            // 🔒 Bloqueo por estado (solo Activa = 1)
            if (orden.StatusOrder != 1)
            {
                TempData["Error"] = "No se puede editar una orden que ya está completada o cancelada.";
                return RedirectToAction("Index");
            }

            // Detalles seleccionados (equipos ya en la orden)
            // ✅ IMPORTANTE: RentalValue debe venir del snapshot (OrderDetails.UnitRentalPrice)
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

                                // ✅ precio pactado (snapshot)
                                RentalValue = detalle.UnitRentalPrice,

                                Quantity = detalle.Quantity,
                                Stock = equipo.Stock
                            }).ToList();

            var cantidadesPorEquipo = detalles.ToDictionary(d => d.EquipmentId, d => d.Quantity);
            var precioPactadoPorEquipo = detalles.ToDictionary(d => d.EquipmentId, d => d.RentalValue);

            var idsSeleccionados = cantidadesPorEquipo.Keys.ToList();

            // Equipos: activos o que ya están en la orden (aunque estén inactivos)
            var equiposBase = _contexto.Equipment
                .Where(e => e.Status == 1 || idsSeleccionados.Contains(e.EquipmentId))
                .ToList();

            var equiposParaModal = equiposBase
                .Select(e =>
                {
                    cantidadesPorEquipo.TryGetValue(e.EquipmentId, out int qty);

                    // ✅ Si el equipo ya estaba en la orden, usar el precio pactado (snapshot)
                    // Si es un equipo nuevo (no estaba en la orden), usar precio actual del inventario
                    decimal precioParaMostrar = e.RentalValue;
                    if (precioPactadoPorEquipo.TryGetValue(e.EquipmentId, out var pactado))
                        precioParaMostrar = pactado;

                    return new OrderDetailDto
                    {
                        EquipmentId = e.EquipmentId,
                        EquipmentName = e.EquipmentName,
                        Brand = e.Brand,
                        Model = e.Model,
                        RentalValue = precioParaMostrar,
                        Stock = e.Stock,
                        Quantity = qty
                    };
                })
                .ToList();

            // Clientes: activos + cliente actual (aunque esté inactivo)
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

                // ✅ Misión 2
                IsDelivery = orden.IsDelivery,
                DeliveryAddress = orden.DeliveryAddress,
                TransportCost = orden.TransportCost,

                EquiposSeleccionados = detalles,
                EquiposDisponibles = equiposParaModal,
                Clientes = clientes
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
            // Equipos seleccionados (desde el form)
            var equiposSeleccionados = model.EquiposSeleccionados?
                .Where(e => e.Quantity > 0)
                .ToList() ?? new List<OrderDetailDto>();

            // =======================
            // Helper: recargar combos + rehidratar cantidades + respetar precio pactado
            // =======================
            void RecargarCombos(CrearRentalOrderViewModel m, List<OrderDetailDto> equiposSel = null, Dictionary<int, decimal> precioPactadoMap = null)
            {
                // Clientes: activos + cliente actual (por si el actual quedó inactivo)
                m.Clientes = _contexto.Clients
                    .Where(c => c.Status == 1 || c.ClientId == m.ClientId)
                    .OrderBy(c => c.FirstName)
                    .ThenBy(c => c.LastName)
                    .Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName
                    }).ToList();

                // Equipos: activos + equipos seleccionados (aunque inactivos)
                var idsSel = equiposSel?.Select(x => x.EquipmentId).Distinct().ToList() ?? new List<int>();

                var equiposBase = _contexto.Equipment
                    .Where(e => e.Status == 1 || idsSel.Contains(e.EquipmentId))
                    .ToList();

                m.EquiposDisponibles = equiposBase
                    .Select(e =>
                    {
                        int qty = 0;
                        if (equiposSel != null)
                        {
                            var found = equiposSel.FirstOrDefault(x => x.EquipmentId == e.EquipmentId);
                            if (found != null) qty = found.Quantity;
                        }

                        // ✅ respetar precio pactado si está disponible
                        decimal precioParaMostrar = e.RentalValue;
                        if (precioPactadoMap != null && precioPactadoMap.TryGetValue(e.EquipmentId, out var pactado))
                            precioParaMostrar = pactado;

                        return new OrderDetailDto
                        {
                            EquipmentId = e.EquipmentId,
                            EquipmentName = e.EquipmentName,
                            Brand = e.Brand,
                            Model = e.Model,
                            RentalValue = precioParaMostrar,
                            Stock = e.Stock,
                            Quantity = qty
                        };
                    })
                    .ToList();

                if (equiposSel != null && equiposSel.Any())
                    m.EquiposSeleccionados = equiposSel;
            }

            // 🔒 Validar que la orden exista y sea editable (anti URL / anti POST manual)
            var ordenDb = await _contexto.RentalOrders.FindAsync(id);
            if (ordenDb == null)
                return HttpNotFound();

            if (ordenDb.StatusOrder != 1)
            {
                TempData["Error"] = "No se puede guardar cambios: la orden ya no está activa.";
                return RedirectToAction("Index");
            }

            // =======================
            // ✅ Misión 2: Validaciones de entrega + normalización
            // =======================
            if (model.IsDelivery)
            {
                if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                    ModelState.AddModelError(nameof(model.DeliveryAddress), "Debe ingresar la dirección de entrega.");

                if (model.TransportCost < 0)
                    ModelState.AddModelError(nameof(model.TransportCost), "El costo de transporte no puede ser negativo.");

                if (!string.IsNullOrWhiteSpace(model.DeliveryAddress))
                    model.DeliveryAddress = model.DeliveryAddress.Trim();
            }
            else
            {
                model.DeliveryAddress = null;
                model.TransportCost = 0m;
            }

            // =======================
            // Validación modelo + equipos
            // =======================
            if (!ModelState.IsValid || !equiposSeleccionados.Any())
            {
                if (!equiposSeleccionados.Any())
                    ModelState.AddModelError("", "Debe ingresar la cantidad de al menos un equipo.");

                // Mapa de precio pactado actual (para que el modal muestre pactado al recargar)
                var precioPactadoActual = _contexto.OrderDetails
                    .Where(d => d.OrderId == id)
                    .GroupBy(d => d.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault().UnitRentalPrice);

                RecargarCombos(model, equiposSeleccionados, precioPactadoActual);
                return View("Edit", model);
            }

            // ✅ Cliente debe seguir activo (o si cambió, el nuevo debe estar activo)
            var clienteDb = _contexto.Clients.FirstOrDefault(c => c.ClientId == model.ClientId);
            if (clienteDb == null || clienteDb.Status != 1)
            {
                ModelState.AddModelError("", "No se puede finalizar: el cliente está inactivo o fue desactivado. Seleccione otro cliente.");

                var precioPactadoActual = _contexto.OrderDetails
                    .Where(d => d.OrderId == id)
                    .GroupBy(d => d.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault().UnitRentalPrice);

                RecargarCombos(model, equiposSeleccionados, precioPactadoActual);
                return View("Edit", model);
            }

            using (var transaction = _contexto.Database.BeginTransaction())
            {
                try
                {
                    // =======================
                    // ✅ Snapshot previo (antes de borrar)
                    // =======================
                    var detallesAnteriores = _contexto.OrderDetails
                        .Where(d => d.OrderId == id)
                        .ToList();

                    // Mapa: EquipmentId -> UnitRentalPrice pactado anterior
                    var precioPactadoAnterior = detallesAnteriores
                        .GroupBy(d => d.EquipmentId)
                        .ToDictionary(g => g.Key, g => g.First().UnitRentalPrice);

                    // =======================
                    // Actualizar cabecera
                    // =======================
                    ordenDb.ClientId = model.ClientId;
                    ordenDb.StartDate = model.StartDate;
                    ordenDb.EndDate = model.EndDate;
                    ordenDb.DescuentoManual = model.DescuentoManual;

                    // ✅ Misión 2
                    ordenDb.IsDelivery = model.IsDelivery;
                    ordenDb.DeliveryAddress = model.DeliveryAddress;
                    ordenDb.TransportCost = model.TransportCost;

                    // Restaurar stock anterior
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

                    // =======================
                    // Aplicar nuevos detalles + descontar stock
                    // ✅ Guardar UnitRentalPrice
                    // =======================
                    foreach (var item in equiposSeleccionados)
                    {
                        var equipo = await _contexto.Equipment.FindAsync(item.EquipmentId);
                        if (equipo == null)
                            throw new InvalidOperationException("El equipo seleccionado no existe.");

                        if (equipo.Stock < item.Quantity)
                        {
                            throw new InvalidOperationException(
                                $"No hay suficiente stock para el equipo: {equipo.EquipmentName}. " +
                                $"Disponibles: {equipo.Stock}, seleccionados: {item.Quantity}."
                            );
                        }

                        // ✅ Precio pactado:
                        // - si el equipo ya estaba antes, conservar el precio anterior
                        // - si es nuevo, tomar el precio actual del inventario
                        decimal unitPrice;
                        if (precioPactadoAnterior.TryGetValue(item.EquipmentId, out var pactado))
                        {
                            unitPrice = pactado;
                        }
                        else
                        {
                            unitPrice = equipo.RentalValue;
                        }

                        _contexto.OrderDetails.Add(new OrderDetailDA
                        {
                            OrderId = id,
                            EquipmentId = item.EquipmentId,
                            Quantity = item.Quantity,
                            UnitRentalPrice = unitPrice
                        });

                        equipo.Stock -= item.Quantity;

                        if (equipo.Stock <= 0)
                        {
                            equipo.Stock = 0;
                            equipo.Status = 2;
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

                    var precioPactadoActual = _contexto.OrderDetails
                        .Where(d => d.OrderId == id)
                        .GroupBy(d => d.EquipmentId)
                        .ToDictionary(g => g.Key, g => g.FirstOrDefault().UnitRentalPrice);

                    RecargarCombos(model, equiposSeleccionados, precioPactadoActual);
                    return View("Edit", model);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Error al guardar los cambios.");

                    var precioPactadoActual = _contexto.OrderDetails
                        .Where(d => d.OrderId == id)
                        .GroupBy(d => d.EquipmentId)
                        .ToDictionary(g => g.Key, g => g.FirstOrDefault().UnitRentalPrice);

                    RecargarCombos(model, equiposSeleccionados, precioPactadoActual);
                    return View("Edit", model);
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

            // ✅ Detalles con precio pactado (snapshot)
            var detalles = (from detalle in _contexto.OrderDetails
                            join equipo in _contexto.Equipment
                                on detalle.EquipmentId equals equipo.EquipmentId
                            where detalle.OrderId == orden.OrderId
                            select new OrderDetailDto
                            {
                                EquipmentName = equipo.EquipmentName,
                                Brand = equipo.Brand,
                                Model = equipo.Model,

                                // ✅ usar UnitRentalPrice guardado en OrderDetails
                                RentalValue = detalle.UnitRentalPrice,

                                Quantity = detalle.Quantity
                            }).ToList();

            var dias = (orden.EndDate - orden.StartDate).Days + 1;
            if (dias < 1) dias = 1;

            // =======================
            // ✅ Cálculos (incluye transporte antes de IVA)
            // =======================
            decimal subtotalEquipos = 0m;
            foreach (var d in detalles)
                subtotalEquipos += d.RentalValue * d.Quantity * dias;

            // ✅ Transporte solo si es entrega
            var transporte = orden.IsDelivery ? orden.TransportCost : 0m;

            var subtotalConTransporte = subtotalEquipos + transporte;

            var iva = Math.Round(subtotalConTransporte * 0.13m, 2);
            var totalBruto = subtotalConTransporte + iva;

            var descuentoPct = orden.DescuentoManual ?? 0m;
            if (descuentoPct < 0m) descuentoPct = 0m;
            if (descuentoPct > 100m) descuentoPct = 100m;

            var montoDescuento = Math.Round(totalBruto * (descuentoPct / 100m), 2);
            var total = totalBruto - montoDescuento;

            // =======================
            // DTO para el PDF
            // =======================
            var dto = new RentalOrderDto
            {
                OrderId = orden.OrderId,
                OrderDate = orden.OrderDate,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                ClientName = $"{cliente.FirstName} {cliente.LastName}",

                Details = detalles,

                CantidadDias = dias,

                // ✅ nuevos campos
                IsDelivery = orden.IsDelivery,
                DeliveryAddress = orden.DeliveryAddress,
                TransportCost = orden.TransportCost,

                DescuentoManual = orden.DescuentoManual,

                // Totales
                Subtotal = subtotalEquipos, // si preferís, también podés guardar subtotalConTransporte en otra prop
                Iva = iva,
                Total = total
            };

            byte[] pdfBytes = ComprobantePdfService.GenerarEnMemoria(dto);
            return File(pdfBytes, "application/pdf", $"Comprobante_Orden_{dto.OrderId}.pdf");
        }


    }
}
