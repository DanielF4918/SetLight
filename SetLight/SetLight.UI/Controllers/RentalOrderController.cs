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

        public ActionResult History(int clientId, int? page, DateTime? desde, DateTime? hasta)
        {
            ClientDto cliente = _obtenerClPorID.Obtener(clientId);
            if (cliente == null)
                return HttpNotFound("Cliente no encontrado");

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
            ViewBag.ClientId = clientId;

            return View(historialPaginado);
        }

        // Listado de órdenes con filtros
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

        // Crear orden (GET)
        public ActionResult Create()
        {
            var clientes = _contexto.Clients
                .Where(c => c.Status == 1)
                .Select(c => new ClientDto
                {
                    ClientId = c.ClientId,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                }).ToList();

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
                }).ToList();

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

        // Crear orden (POST)
        [HttpPost]
        public async Task<ActionResult> Create(CrearRentalOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var equiposSeleccionados = model.EquiposSeleccionados?.Where(e => e.Quantity > 0).ToList();

                if (equiposSeleccionados == null || !equiposSeleccionados.Any())
                {
                    ModelState.AddModelError("", "Debe ingresar la cantidad de al menos un equipo.");

                    model.Clientes = _contexto.Clients
                        .Where(c => c.Status == 1)
                        .Select(c => new ClientDto
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
                            Quantity = 0
                        }).ToList();

                    return View(model);
                }

                string correoUsuario = User.Identity?.Name ?? "";
                var empleado = _contexto.Empleado.FirstOrDefault(e => e.CorreoElectronico == correoUsuario && e.Estado);
                int? idEmpleado = empleado?.IdEmpleado;

                var nuevaOrden = new RentalOrderDto
                {
                    ClientId = model.ClientId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    StatusOrder = model.StatusOrder,
                    EmpleadoId = idEmpleado,
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

                var crearLN = new CrearRentalOrderLN(_crearOrdenAD);
                await crearLN.Guardar(nuevaOrden);

                var ordenGuardada = _contexto.RentalOrders
                    .OrderByDescending(o => o.OrderId)
                    .FirstOrDefault(o => o.ClientId == model.ClientId && o.StartDate == model.StartDate);

                if (ordenGuardada != null && (ordenGuardada.StatusOrder == 1 || ordenGuardada.StatusOrder == 2))
                {
                    var cliente = _contexto.Clients.FirstOrDefault(c => c.ClientId == model.ClientId);

                    var ordenParaPDF = new RentalOrderDto
                    {
                        OrderId = ordenGuardada.OrderId,
                        OrderDate = ordenGuardada.OrderDate,
                        StartDate = ordenGuardada.StartDate,
                        EndDate = ordenGuardada.EndDate,
                        ClientName = cliente.FirstName + " " + cliente.LastName,
                        Details = equiposSeleccionados
                    };

                    byte[] pdfBytes = ComprobantePdfService.GenerarEnMemoria(ordenParaPDF);
                    string fileName = $"Orden_{ordenParaPDF.OrderId}.pdf";
                    ordenGuardada.RutaComprobante = fileName;
                    await _contexto.SaveChangesAsync();
                }

                return RedirectToAction("Index");
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
                    Quantity = 0,
                    Stock = e.Stock
                }).ToList();

            return View(model);
        }

        // GET: RentalOrder/Edit/5
        public ActionResult Edit(int id)
        {
            var orden = _contexto.RentalOrders.FirstOrDefault(o => o.OrderId == id);
            if (orden == null)
                return HttpNotFound();

            // Detalles seleccionados en la orden
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

            // Diccionario para saber cuántas unidades tiene cada equipo en la orden
            var cantidadesPorEquipo = detalles.ToDictionary(d => d.EquipmentId, d => d.Quantity);

            // Lista de IDs ya seleccionados
            var idsSeleccionados = cantidadesPorEquipo.Keys.ToList();

            // 1) Traemos de la BD los equipos que nos interesan (esto sí es LINQ to Entities)
            var equiposBase = _contexto.Equipment
                .Where(e => e.Status == 1 || idsSeleccionados.Contains(e.EquipmentId))
                .ToList();   // 👈 aquí ya pasamos a memoria

            // 2) Ahora sí usamos el Dictionary en memoria para armar el DTO del modal
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
                        Quantity = qty // 0 si no estaba en la orden, o la cantidad actual si sí estaba
                    };
                })
                .ToList();


            var viewModel = new CrearRentalOrderViewModel
            {
                OrderId = orden.OrderId,
                ClientId = orden.ClientId,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                StatusOrder = orden.StatusOrder,
                DescuentoManual = orden.DescuentoManual,

                EquiposSeleccionados = detalles,      // para la tabla de abajo
                EquiposDisponibles = equiposParaModal, // para el modal

                Clientes = _contexto.Clients
                    .Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName
                    }).ToList()
            };

            return View("Edit", viewModel);
        }


        //POST: RentalOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CrearRentalOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
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

                    foreach (var item in model.EquiposSeleccionados.Where(e => e.Quantity > 0))
                    {
                        _contexto.OrderDetails.Add(new OrderDetailDA
                        {
                            OrderId = id,
                            EquipmentId = item.EquipmentId,
                            Quantity = item.Quantity
                        });

                        var equipo = await _contexto.Equipment.FindAsync(item.EquipmentId);
                        if (equipo != null)
                        {
                            if (equipo.Stock < item.Quantity)
                                throw new InvalidOperationException($"Stock insuficiente para {equipo.EquipmentName}");

                            equipo.Stock -= item.Quantity;

                            if (equipo.Stock <= 0)
                            {
                                equipo.Stock = 0;
                                equipo.Status = 2; // Inactivo
                            }
                        }
                    }

                    await _contexto.SaveChangesAsync();
                    transaction.Commit();

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Error al guardar los cambios.");
                }
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

            return View(model);
        }

        // 🔴 NUEVO: Cancelar orden
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

                    // Solo se pueden cancelar órdenes activas
                    if (orden.StatusOrder != 1)
                        return RedirectToAction("Index");

                    // Devolver stock de los equipos de la orden
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

                    // 3 = Cancelada
                    orden.StatusOrder = 3;

                    await _contexto.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    // Aquí podrías loguear el error o usar TempData para un mensaje
                }
            }

            return RedirectToAction("Index");
        }

        // GET: VerComprobante
        public ActionResult VerComprobante(int id)
        {
            var orden = _contexto.RentalOrders.Find(id);
            if (orden == null)
                return HttpNotFound("Orden no encontrada");

            var cliente = _contexto.Clients.FirstOrDefault(c => c.ClientId == orden.ClientId);
            if (cliente == null)
                return HttpNotFound("Cliente no encontrado");

            // Detalles de la orden
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

            // ====== Cálculos de alquiler (mismo criterio que en el JS) ======
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

            // DTO completo para el PDF
            var dto = new RentalOrderDto
            {
                OrderId = orden.OrderId,
                OrderDate = orden.OrderDate,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                ClientName = $"{cliente.FirstName} {cliente.LastName}",
                Details = detalles,

                // 👇 campos de descuento y totales
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
