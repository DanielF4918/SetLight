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

namespace SetLight.UI.Controllers
{
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

        // Listado de órdenes
        public ActionResult Index()
        {
            var ordenes = _listarOrdenesAD.Obtener();
            return View(ordenes);
        }

        // Crear orden (GET)
        public ActionResult Create()
        {
            var clientes = _contexto.Clients
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
                            Quantity = 0
                        }).ToList();

                    return View(model);
                }

                var nuevaOrden = new RentalOrderDto
                {
                    ClientId = model.ClientId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    StatusOrder = model.StatusOrder,
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


            var viewModel = new CrearRentalOrderViewModel
            {
                OrderId = orden.OrderId,
                ClientId = orden.ClientId,
                StartDate = orden.StartDate,
                EndDate = orden.EndDate,
                StatusOrder = orden.StatusOrder,
                EquiposSeleccionados = detalles,
                Clientes = _contexto.Clients.Select(c => new ClientDto
                {
                    ClientId = c.ClientId,
                    FirstName = c.FirstName,
                    LastName = c.LastName
                }).ToList()
            };

            viewModel.EquiposDisponibles = _contexto.Equipment
            .Where(e => e.Status == 1 && e.Stock > 0)
            .Select(e => new OrderDetailDto
            {
                EquipmentId = e.EquipmentId,
                EquipmentName = e.EquipmentName,
                Brand = e.Brand,
                Model = e.Model,
                RentalValue = e.RentalValue,
                Stock = e.Stock,
                Quantity = 0
            }).ToList();


            return View(viewModel);
        }

        //POST: RentalOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CrearRentalOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar combos por si hay error
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
                    var orden = _contexto.RentalOrders.Find(id);
                    if (orden == null)
                        return HttpNotFound();

                    orden.ClientId = model.ClientId;
                    orden.StartDate = model.StartDate;
                    orden.EndDate = model.EndDate;
                    orden.StatusOrder = model.StatusOrder;
                    orden.OrderDate = DateTime.Now;

                    var detallesAntiguos = _contexto.OrderDetails.Where(d => d.OrderId == id).ToList();

                    foreach (var detalle in detallesAntiguos)
                    {
                        var equipo = _contexto.Equipment.FirstOrDefault(e => e.EquipmentId == detalle.EquipmentId);
                        if (equipo != null)
                        {
                            equipo.Stock += detalle.Quantity;

                            if (equipo.Stock > 0 && equipo.Status == 2)
                                equipo.Status = 1;
                        }
                    }

                    _contexto.OrderDetails.RemoveRange(detallesAntiguos);
                    await _contexto.SaveChangesAsync();

                    foreach (var nuevo in model.EquiposSeleccionados)
                    {
                        _contexto.OrderDetails.Add(new OrderDetailDA
                        {
                            OrderId = orden.OrderId,
                            EquipmentId = nuevo.EquipmentId,
                            Quantity = nuevo.Quantity
                        });

                        var equipo = _contexto.Equipment.FirstOrDefault(e => e.EquipmentId == nuevo.EquipmentId);
                        if (equipo != null)
                        {
                            if (equipo.Stock < nuevo.Quantity)
                                throw new InvalidOperationException($"Stock insuficiente para {equipo.EquipmentName}");

                            equipo.Stock -= nuevo.Quantity;

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
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Ocurrió un error al guardar los cambios: " + ex.Message);
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





    }
}
