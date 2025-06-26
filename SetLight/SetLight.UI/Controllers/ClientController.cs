using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Client.CreateClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.EditClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.ListClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.ObtenerClPorId;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.EditarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.LogicaDeNegocio.Client.CreateClient;
using SetLight.LogicaDeNegocio.Client.EditClient;
using SetLight.LogicaDeNegocio.Client.ListClient;
using SetLight.LogicaDeNegocio.Client.ObtenerClPorIDLN;


namespace SetLight.UI.Controllers
{
    public class ClientController : Controller
    {

        private IListarClientLN _listarClientLN;
        private ICrearClientLN _crearClientLN;
        private IObtenerClPorIDLN _obtenerClPorIDLN;
        private IEditClientLN _editClientLN;


        public ClientController()
        {
            _listarClientLN = new ListarClientLN();
            _crearClientLN = new CrearClientLN();
            _obtenerClPorIDLN = new ObtenerClPorIDLN();
            _editClientLN = new EditClientLN();
        }



        // GET: Client/ListarClient
        public ActionResult ListarClient(string nombre, string telefono, string correo, string status)
        {
            var lista = _listarClientLN.Obtener();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                string nombreLower = nombre.ToLower();
                lista = lista.Where(c =>
                    ((c.FirstName + " " + c.LastName)?.ToLower().Contains(nombreLower) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(telefono))
            {
                string telefonoLower = telefono.ToLower();
                lista = lista.Where(c =>
                    c.Phone != null && c.Phone.ToLower().Contains(telefonoLower)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(correo))
            {
                string correoLower = correo.ToLower();
                lista = lista.Where(c =>
                    c.Email != null && c.Email.ToLower().Contains(correoLower)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out int estadoInt))
            {
                lista = lista.Where(c => c.Status == estadoInt).ToList();
            }

            ViewBag.NombreBuscado = nombre;
            ViewBag.TelefonoBuscado = telefono;
            ViewBag.CorreoBuscado = correo;
            ViewBag.Estados = new List<SelectListItem>
    {
        new SelectListItem { Text = "Activo", Value = "1", Selected = (status == "1") },
        new SelectListItem { Text = "Inactivo", Value = "0", Selected = (status == "0") }
    };

            return View(lista);
        }




        // GET: Client/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Client/Create
        public ActionResult Create()
        {
            return View();
        }
        // POST: Client/Create
        [HttpPost]
        public async Task<ActionResult> Create(ClientDto clientGuardar)
        {
            if (!ModelState.IsValid)
                return View(clientGuardar);

            try
            {
                await _crearClientLN.Guardar(clientGuardar);
                return RedirectToAction("ListarClient");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar cliente: " + ex.Message);
                return View(clientGuardar);
            }
        }
        // GET: Client/Edit/5
        public ActionResult Edit(int id)
        {
            var cliente = _obtenerClPorIDLN.Obtener(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }

            return View("EditClient", cliente);
        }

        // POST: Client/Edit/5
        [HttpPost]
        public ActionResult Edit(ClientDto model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditClient", model);
            }

            _editClientLN.Actualizar(model);


            return RedirectToAction("ListarClient"); 
        }
    

        // GET: Client/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Client/Delete/5
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

        // GET: Equipment/Activar/5
        public ActionResult Activar(int id)
        {
            var equipo = _obtenerClPorIDLN.Obtener(id);
            equipo.Status = 1; // Activo
            _editClientLN.Actualizar(equipo);
            return RedirectToAction("ListarClient");
        }

        // GET: Equipment/Inactivar/5
        public ActionResult Inactivar(int id)
        {
            var equipo = _obtenerClPorIDLN.Obtener(id);
            equipo.Status = 3; // Inactivo
            _editClientLN.Actualizar(equipo);
            return RedirectToAction("ListarClient");
        }


        public PartialViewResult BuscarClientesModal(string filtro)
        {
            using (var contexto = new Contexto())
            {
                var clientes = contexto.Clients
                    .Where(c => filtro == null || c.FirstName.Contains(filtro) || c.LastName.Contains(filtro))
                    .Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Email = c.Email,
                        Phone = c.Phone
                    }).ToList();

                return PartialView("_SeleccionarClientePartial", clientes);
            }
        }

    }


}