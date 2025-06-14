using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Client.CreateClient;
using SetLight.Abstracciones.LogicaDeNegocio.Client.ListClient;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.LogicaDeNegocio.Client.CreateClient;
using SetLight.LogicaDeNegocio.Client.ListClient;


namespace SetLight.UI.Controllers
{
    public class ClientController : Controller
    {

        private readonly IListarClientLN _listarClientLN;
        private readonly ICrearClientLN _crearClientLN;

        public ClientController()
        {
            _listarClientLN = new ListarClientLN();
            _crearClientLN = new CrearClientLN();
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
            return View();
        }

        // POST: Client/Edit/5
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
    }
}
