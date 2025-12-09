using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SetLight.Abstracciones.LogicaDeNegocio.Client.CreateClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.EditClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.ListClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.ObtenerClPorId;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.LogicaDeNegocio.Client.CreateClient;
using SetLight.LogicaDeNegocio.Client.EditClient;
using SetLight.LogicaDeNegocio.Client.ListClient;
using SetLight.LogicaDeNegocio.Client.ObtenerClPorIDLN;

namespace SetLight.UI.Controllers
{
    [Authorize(Roles = "Administrador,Colaborador,Tecnico")]
    public class ClientController : Controller
    {
        private readonly IListarClientLN _listarClientLN;
        private readonly ICrearClientLN _crearClientLN;
        private readonly IObtenerClPorIDLN _obtenerClPorIDLN;
        private readonly IEditClientLN _editClientLN;

        public ClientController()
        {
            _listarClientLN = new ListarClientLN();
            _crearClientLN = new CrearClientLN();
            _obtenerClPorIDLN = new ObtenerClPorIDLN();
            _editClientLN = new EditClientLN();
        }

        // GET: Client/ListarClient
        public ActionResult ListarClient(string nombre, string telefono, string correo, string empresa, string status, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

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
                    (c.Phone != null && c.Phone.ToLower().Contains(telefonoLower)) ||
                    (c.EmpresaTelefono != null && c.EmpresaTelefono.ToLower().Contains(telefonoLower))
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(correo))
            {
                string correoLower = correo.ToLower();
                lista = lista.Where(c =>
                    c.Email != null && c.Email.ToLower().Contains(correoLower)
                ).ToList();
            }
            if (!string.IsNullOrWhiteSpace(empresa))
            {
                string empresaLower = empresa.ToLower();
                lista = lista.Where(c =>
                    c.EmpresaNombre != null &&
                    c.EmpresaNombre.ToLower().Contains(empresaLower)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out int estadoInt))
            {
                lista = lista.Where(c => c.Status == estadoInt).ToList();
            }


            ViewBag.NombreBuscado = nombre;
            ViewBag.TelefonoBuscado = telefono;
            ViewBag.CorreoBuscado = correo;
            ViewBag.EmpresaBuscada = empresa;
            ViewBag.Estados = new List<SelectListItem>
            {
                new SelectListItem { Text = "Activo",   Value = "1", Selected = (status == "1") },
                new SelectListItem { Text = "Inactivo", Value = "0", Selected = (status == "0") }
            };
            var pagedList = lista
        .OrderBy(c => c.FirstName)
        .ToPagedList(pageNumber, pageSize);


            return View(pagedList);
        }

        // GET: Client/Details/5
        public ActionResult Details(int id)
        {
            var cliente = _obtenerClPorIDLN.Obtener(id);
            if (cliente == null)
                return HttpNotFound();

            // La vista Details podrá mostrar también EmpresaNombre y EmpresaTelefono
            return View(cliente);
        }

        // GET: Client/Create
        public ActionResult Create()
        {
            // devolvemos un dto vacío por si la vista usa helpers strongly-typed
            var model = new ClientDto();
            return View(model);
        }

        // POST: Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ClientDto clientGuardar)
        {
            // Aseguramos que el estado sea Activo al crear
            clientGuardar.Status = 1;

            // Si las validaciones de DataAnnotations FALLAN,
            // volvemos a la vista y mostramos los mensajes por campo.
            if (!ModelState.IsValid)
                return View(clientGuardar);

            try
            {
                await _crearClientLN.Guardar(clientGuardar);
                return RedirectToAction("ListarClient");
            }
            catch (Exception)
            {
              
                // ModelState.AddModelError("", "Ocurrió un error al guardar el cliente. Inténtelo de nuevo.");

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

            // La vista EditClient ya recibe el ClientDto con EmpresaNombre/EmpresaTelefono
            return View("EditClient", cliente);
        }

        // POST: Client/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ClientDto model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditClient", model);
            }

            // Conservar el estado original del cliente
            var clienteBD = _obtenerClPorIDLN.Obtener(model.ClientId);
            if (clienteBD == null)
            {
                return HttpNotFound();
            }

            // Mantener el Status que ya tenía en la base de datos
            model.Status = clienteBD.Status;

            _editClientLN.Actualizar(model);
            return RedirectToAction("ListarClient");
        }


        // GET: Client/Delete/5
        public ActionResult Delete(int id)
        {
            // No está implementado aún, lo dejamos como estaba
            return View();
        }

        // POST: Client/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: lógica de borrado si se implementa
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Client/Activar/5
        public ActionResult Activar(int id)
        {
            var cliente = _obtenerClPorIDLN.Obtener(id);
            if (cliente != null)
            {
                cliente.Status = 1; // Activo
                _editClientLN.Actualizar(cliente);
            }

            return RedirectToAction("ListarClient");
        }

        // GET: Client/Inactivar/5
        public ActionResult Inactivar(int id)
        {
            var cliente = _obtenerClPorIDLN.Obtener(id);
            if (cliente != null)
            {
                cliente.Status = 3; // Inactivo
                _editClientLN.Actualizar(cliente);
            }

            return RedirectToAction("ListarClient");
        }

        public PartialViewResult BuscarClientesModal(string filtro)
        {
            using (var contexto = new Contexto())
            {
                var query = contexto.Clients.AsQueryable();

                // ✅ Solo clientes ACTIVOS
                query = query.Where(c => c.Status == 1);

                // Normalizamos el filtro
                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    var term = filtro.Trim().ToLower();

                    query = query.Where(c =>
                        (c.FirstName != null && c.FirstName.ToLower().Contains(term)) ||
                        (c.LastName != null && c.LastName.ToLower().Contains(term)) ||
                        (c.EmpresaNombre != null && c.EmpresaNombre.ToLower().Contains(term)) ||
                        (c.Email != null && c.Email.ToLower().Contains(term)) ||
                        (c.Phone != null && c.Phone.ToLower().Contains(term))
                    );
                }

                var clientes = query
                    .Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Email = c.Email,
                        Phone = c.Phone,
                        EmpresaNombre = c.EmpresaNombre,
                        EmpresaTelefono = c.EmpresaTelefono
                    })
                    .ToList();

                return PartialView("_SeleccionarClientePartial", clientes);
            }
        }

        // GET: Client/ModalDetallesCliente/5
        public ActionResult ModalDetallesCliente(int id)
        {
            // Usamos tu lógica ya existente
            var cliente = _obtenerClPorIDLN.Obtener(id);
            if (cliente == null)
            {
                return HttpNotFound();
            }

            // Renderiza la vista parcial reutilizable
            return PartialView("_ClienteDetallesModal", cliente);
        }




    }
}
