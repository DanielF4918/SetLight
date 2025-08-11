using System;
using System.Linq;
using System.Web.Mvc;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.AccesoADatos.Clientes.ObtenerClPorID;
using SetLight.AccesoADatos.RentalOrder;
using SetLight.Abstracciones.AccesoADatos.RentalOrder.CrearRentalOrder;
using SetLight.Abstracciones.ViewModels;
using System.Threading.Tasks;
using SetLight.AccesoADatos.rentalorder.EditRentalOrder;
using SetLight.AccesoADatos.rentalorder.ObtenerROPorId;
using SetLight.AccesoADatos.Modelos;
using SetLight.AccesoADatos.Equipment.ObtenerEqPorID;
using SetLight.LogicaDeNegocio.Services;
using System.Collections.Generic;

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

        public ActionResult History(int clientId)
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
                             }).ToList();

            ViewBag.ClientName = cliente.FirstName + " " + cliente.LastName;
            return View(historial);
        }

        public ActionResult Index()
        {
            var ordenes = (from orden in _contexto.RentalOrders
                           join cliente in _contexto.Clients on orden.ClientId equals cliente.ClientId
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
                               ClientName = cliente.FirstName + " " + cliente.LastName,
                               EmpleadoNombreCompleto = empleado != null
                                    ? empleado.Nombre + " " + empleado.Apellido
                                    : "No asignado",
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
                           }).ToList();

            return View(ordenes);
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

            var detalles = (from detalle in _contexto.OrderDetails
                            where detalle.OrderId == id && detalle.Quantity > 0
                            join equipo in _contexto.Equipment on detalle.EquipmentId equals equipo.EquipmentId
                            select new
                            {
                                equipo,
                                detalle.Quantity
                            }).ToList();


            var equiposDb = _contexto.Equipment.Where(e => e.Status == 1).ToList();


            var disponibles = equiposDb.Select(e => {
                var yaAsignado = detalles.FirstOrDefault(d => d.equipo.EquipmentId == e.EquipmentId)?.Quantity ?? 0;
                return new OrderDetailDto
                {
                    EquipmentId = e.EquipmentId,
                    EquipmentName = e.EquipmentName,
                    Brand = e.Brand,
                    Model = e.Model,
                    RentalValue = e.RentalValue,
                    Stock = e.Stock,
                    Quantity = yaAsignado,
                    CantidadMaxima = e.Stock + yaAsignado
                };
            }).ToList();

            var viewModel = new CrearRentalOrderViewModel
            {
                OrderId = orden.OrderId,
                ClientId = orden.ClientId,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                StatusOrder = orden.StatusOrder,
                DescuentoManual = orden.DescuentoManual,
                EquiposSeleccionados = disponibles.Where(d => d.Quantity > 0).ToList(),
                EquiposDisponibles = disponibles,
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
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Error al guardar los cambios: " + ex.Message);
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



        //GET: VerComprobante
        public ActionResult VerComprobante(int id)
        {
            var orden = _contexto.RentalOrders.Find(id);
            if (orden == null)
                return HttpNotFound("Orden no encontrada");

            var cliente = _contexto.Clients.FirstOrDefault(c => c.ClientId == orden.ClientId);
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

            int cantidadDias = (orden.EndDate - orden.StartDate).Days;
            if (cantidadDias <= 0) cantidadDias = 1;

            decimal subtotal = detalles.Sum(d => d.RentalValue * d.Quantity * cantidadDias);
            decimal descuento = orden.DescuentoManual ?? 0;
            decimal iva = (subtotal - descuento) * 0.13m;
            decimal total = subtotal - descuento + iva;

            var dto = new RentalOrderDto
            {
                OrderId = orden.OrderId,
                OrderDate = orden.OrderDate,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                ClientName = cliente.FirstName + " " + cliente.LastName,
                Details = detalles,
                DescuentoManual = descuento,
                CantidadDias = cantidadDias,
                Subtotal = subtotal,
                Iva = iva,
                Total = total
            };

            byte[] pdfBytes = ComprobantePdfService.GenerarEnMemoria(dto);
            return File(pdfBytes, "application/pdf", $"Comprobante_Orden_{dto.OrderId}.pdf");
        }

    }
}