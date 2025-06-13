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

namespace SetLight.UI.Controllers
{
    public class RentalOrderController : Controller
    {
        private Contexto _contexto;
        private ObtenerClPorIDAD _obtenerClPorID;
        private ListarRentalOrderAD _listarOrdenesAD;
        private CrearRentalOrderAD _crearOrdenAD;

        public RentalOrderController()
        {
            _contexto = new Contexto();
            _obtenerClPorID = new ObtenerClPorIDAD();
            _listarOrdenesAD = new ListarRentalOrderAD();
            _crearOrdenAD = new CrearRentalOrderAD();
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


    }
}
