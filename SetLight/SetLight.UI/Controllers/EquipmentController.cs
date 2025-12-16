using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.CrearEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.EditarEquipment;
using SetLight.Abstracciones.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;
using SetLight.LogicaDeNegocio.Equipment.CrearEquipment;
using SetLight.LogicaDeNegocio.Equipment.EditarEquipment;
using SetLight.LogicaDeNegocio.Equipment.ListarEquipment;
using SetLight.LogicaDeNegocio.Equipment.ObtenerEqPorID;

namespace SetLight.UI.Controllers
{
    [Authorize(Roles = "Administrador,Colaborador,Tecnico")]
    public class EquipmentController : Controller
    {
        private IListarEquipmentLN _listarEquipmentLN;
        private IObtenerEqPorIDLN _ObtenerEqPorIDLN;
        private ICrearEquipmentLN _crearEquipmentLN;
        private IEquipmentLN _equipmentLN;

        private const string UploadRoot = "~/Uploads/Equipment";

        public EquipmentController()
        {
            _listarEquipmentLN = new ListarEquipmentLN();
            _crearEquipmentLN = new CrearEquipmentLN();
            _ObtenerEqPorIDLN = new ObtenerEqPorIDLN();
            _equipmentLN = new EditarEquipmentLN();
        }

        // GET: Equipment/ListarEquipment
        public ActionResult ListarEquipment(string Nombre, int? CategoriaId, int? Estado, int? page)
        {
            var lista = _listarEquipmentLN.Obtener();

            using (var contexto = new Contexto())
            {
                // Alquilados por equipo (órdenes activas)
                var alquiladosPorEquipo = contexto.RentalOrders
                    .Where(o => o.StatusOrder == 1)
                    .SelectMany(o => o.OrderDetails)
                    .GroupBy(d => d.EquipmentId)
                    .Select(g => new
                    {
                        EquipmentId = g.Key,
                        Cant = g.Sum(x => (int?)x.Quantity) ?? 0
                    })
                    .ToDictionary(x => x.EquipmentId, x => x.Cant);

                // Mantenimientos por equipo - SOLO pendientes (status 0)
                var mantenimientoPorEquipo = contexto.Maintenance
                    .Where(m =>
    m.MaintenanceStatus == 0 &&
    m.MaintenanceType != 4)
                    // 0 = Pendiente
                    .GroupBy(m => m.EquipmentId)
                    .Select(g => new
                    {
                        EquipmentId = g.Key,
                        Cant = g.Count()
                    })
                    .ToDictionary(x => x.EquipmentId, x => x.Cant);
                // Faltantes: mantenimiento tipo 4 (equipo no devuelto / perdido) y estado pendiente
                var faltantesPorEquipo = contexto.Maintenance
                    .Where(m =>
    m.MaintenanceType == 4 &&
    m.MaintenanceStatus == 0 &&
    m.EquipmentId > 0)

                    .GroupBy(m => m.EquipmentId)
                    .Select(g => new
                    {
                        EquipmentId = g.Key,
                        Cant = g.Count()
                    })
                    .ToDictionary(x => x.EquipmentId, x => x.Cant);

                foreach (var e in lista)
                {
                    e.Alquilados = alquiladosPorEquipo.TryGetValue(e.EquipmentId, out var cantAlq)
                        ? cantAlq
                        : 0;

                    e.EnMantenimiento = mantenimientoPorEquipo.TryGetValue(e.EquipmentId, out var cantMant)
                        ? cantMant
                        : 0;

                    // Stock en BD ya refleja lo disponible, no restamos alquilados otra vez
                    e.Disponibles = e.Stock < 0 ? 0 : e.Stock;

                    // Faltantes
                    e.Faltantes = faltantesPorEquipo.TryGetValue(e.EquipmentId, out var cantFalt)
                        ? cantFalt
                        : 0;

                    // Mostrar AGOTADO solo si stock = 0 y el equipo está activo
                    if (e.Disponibles == 0 && e.Status == 1)
                    {
                        e.Status = 2; // Agotado (solo visual, no BD)
                    }

                }


                // ----- Combo de categorías (Tipo) -----
                var categoriasBD = contexto.EqCategory
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                var categorias = new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = "0",
                        Text = "Todos",
                        Selected = !CategoriaId.HasValue || CategoriaId == 0
                    }
                };

                categorias.AddRange(
                    categoriasBD.Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName,
                        Selected = CategoriaId.HasValue && CategoriaId.Value == c.CategoryId
                    })
                );

                ViewBag.Categorias = categorias;
            }

            // -------- Filtros --------
            if (!string.IsNullOrWhiteSpace(Nombre))
            {
                var n = Nombre.Trim().ToLower();
                lista = lista.Where(e =>
                       (!string.IsNullOrEmpty(e.EquipmentName) && e.EquipmentName.ToLower().Contains(n))
                    || (!string.IsNullOrEmpty(e.Brand) && e.Brand.ToLower().Contains(n))
                    || (!string.IsNullOrEmpty(e.Model) && e.Model.ToLower().Contains(n))
                ).ToList();
            }

            if (CategoriaId.HasValue && CategoriaId.Value != 0)
                lista = lista.Where(e => e.CategoryId == CategoriaId.Value).ToList();

            if (Estado.HasValue && Estado.Value != 0)
            {
                switch (Estado.Value)
                {
                    case 1: // Activo
                    case 3: // Inactivo
                        lista = lista.Where(e => e.Status == Estado.Value).ToList();
                        break;
                    case 2: // Alquilado
                        lista = lista.Where(e => e.Alquilados > 0).ToList();
                        break;
                    case 4: // En mantenimiento 
                        lista = lista.Where(e => e.EnMantenimiento > 0).ToList();
                        break;
                }
            }

            // Combo de estados
            ViewBag.Estados = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Todos",            Selected = !Estado.HasValue || Estado == 0 },
                new SelectListItem { Value = "1", Text = "Activo",           Selected = Estado == 1 },
                new SelectListItem { Value = "2", Text = "Alquilado",        Selected = Estado == 2 },
                new SelectListItem { Value = "3", Text = "Inactivo",         Selected = Estado == 3 },
                new SelectListItem { Value = "4", Text = "En mantenimiento", Selected = Estado == 4 }
            };

            ViewBag.NombreBuscado = Nombre;
            ViewBag.PlaceholderImagen = Url.Content("~/content/img/placeholder-equipment.png");

            lista = lista
    .OrderBy(e => (e.EquipmentName ?? "").Trim().ToLower())
    .ToList();

            int pageNumber = page ?? 1;
            int pageSize = 12;

            return View(lista.ToPagedList(pageNumber, pageSize));
        }

        // --- el resto de tu controller queda igual ---

        public ActionResult Details(int id)
        {
            List<EquipmentDto> LaListaEquipment = _listarEquipmentLN.Obtener();
            return View(LaListaEquipment);
        }

        public ActionResult CrearEquipment()
        {
            using (var contexto = new Contexto())
            {
                var categorias = contexto.EqCategory
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName
                    }).ToList();

                ViewBag.Categorias = categorias;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearEquipment(EquipmentDto equipmentguardar, HttpPostedFileBase imagen)
        {
            if (!ModelState.IsValid)
            {
                CargarCategoriasEnViewBag(equipmentguardar.CategoryId);
                return View(equipmentguardar);
            }

            string rutaGuardada = null;

            try
            {
                if (imagen != null && imagen.ContentLength > 0)
                {
                    var ext = Path.GetExtension(imagen.FileName)?.ToLowerInvariant();
                    var okExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!okExt.Contains(ext))
                    {
                        ModelState.AddModelError("", "Formato inválido. Solo se permiten .jpg, .jpeg, .png, .webp");
                        CargarCategoriasEnViewBag(equipmentguardar.CategoryId);
                        return View(equipmentguardar);
                    }

                    if (imagen.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "La imagen supera el tamaño máximo permitido (5 MB).");
                        CargarCategoriasEnViewBag(equipmentguardar.CategoryId);
                        return View(equipmentguardar);
                    }

                    var carpetaFisica = Server.MapPath(UploadRoot);
                    Directory.CreateDirectory(carpetaFisica);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var rutaFisica = Path.Combine(carpetaFisica, fileName);

                    imagen.SaveAs(rutaFisica);

                    rutaGuardada = Url.Content($"{UploadRoot}/{fileName}");
                    equipmentguardar.ImageUrl = rutaGuardada;
                }

                await _crearEquipmentLN.Guardar(equipmentguardar);
                TempData["Ok"] = "Equipo creado correctamente.";
                return RedirectToAction("ListarEquipment");
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(rutaGuardada))
                {
                    try
                    {
                        var rutaFisica = Server.MapPath(rutaGuardada);
                        if (System.IO.File.Exists(rutaFisica))
                            System.IO.File.Delete(rutaFisica);
                    }
                    catch { }
                }

                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                CargarCategoriasEnViewBag(equipmentguardar.CategoryId);
                return View(equipmentguardar);
            }
        }

        private void CargarCategoriasEnViewBag(int? seleccion = null)
        {
            using (var contexto = new Contexto())
            {
                ViewBag.Categorias = contexto.EqCategory
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName,
                        Selected = (seleccion.HasValue && seleccion.Value == c.CategoryId)
                    }).ToList();
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var elEquipment = _ObtenerEqPorIDLN.Obtener(id);
            if (elEquipment == null) return HttpNotFound();

            using (var contexto = new Contexto())
            {
                ViewBag.Categorias = contexto.EqCategory
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName,
                        Selected = (c.CategoryId == elEquipment.CategoryId)
                    }).ToList();
            }

            return View("EditEquipment", elEquipment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            EquipmentDto elEquipment,
            HttpPostedFileBase nuevaImagen,
            bool? eliminarImagen
        )
        {
            if (!ModelState.IsValid)
            {
                CargarCategorias(elEquipment.CategoryId);
                return View("EditEquipment", elEquipment);
            }

            var actual = _ObtenerEqPorIDLN.Obtener(elEquipment.EquipmentId);
            if (actual == null)
            {
                ModelState.AddModelError("", "El equipo no existe.");
                CargarCategorias(elEquipment.CategoryId);
                return View("EditEquipment", elEquipment);
            }

            elEquipment.Status = actual.Status;

            var oldUrl = actual.ImageUrl;
            string newUrlGuardada = null;

            try
            {
                if (eliminarImagen == true)
                {
                    elEquipment.ImageUrl = null;
                }
                else if (nuevaImagen != null && nuevaImagen.ContentLength > 0)
                {
                    var ext = Path.GetExtension(nuevaImagen.FileName)?.ToLowerInvariant();
                    var okExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    if (!okExt.Contains(ext))
                    {
                        ModelState.AddModelError("", "Formato inválido. Solo se permite .jpg, .jpeg, .png o .webp");
                        CargarCategorias(elEquipment.CategoryId);
                        return View("EditEquipment", elEquipment);
                    }
                    if (nuevaImagen.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "La imagen supera 5 MB.");
                        CargarCategorias(elEquipment.CategoryId);
                        return View("EditEquipment", elEquipment);
                    }

                    const string UploadRoot = "~/Uploads/Equipment";
                    var carpetaFisica = Server.MapPath(UploadRoot);
                    Directory.CreateDirectory(carpetaFisica);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var rutaFisica = Path.Combine(carpetaFisica, fileName);
                    nuevaImagen.SaveAs(rutaFisica);

                    newUrlGuardada = Url.Content($"{UploadRoot}/{fileName}");
                    elEquipment.ImageUrl = newUrlGuardada;
                }
                else
                {
                    elEquipment.ImageUrl = oldUrl;
                }

                var filas = _equipmentLN.Actualizar(elEquipment);
                if (filas <= 0)
                {
                    BorrarArchivoFisicoSilencioso(newUrlGuardada);
                    ModelState.AddModelError("", "No se pudo actualizar el equipo.");
                    CargarCategorias(elEquipment.CategoryId);
                    return View("EditEquipment", elEquipment);
                }

                if ((eliminarImagen == true || newUrlGuardada != null) && !string.IsNullOrWhiteSpace(oldUrl))
                {
                    BorrarArchivoFisicoSilencioso(oldUrl);
                }

                TempData["Ok"] = "Equipo actualizado correctamente.";
                return RedirectToAction("ListarEquipment");
            }
            catch (Exception ex)
            {
                BorrarArchivoFisicoSilencioso(newUrlGuardada);

                ModelState.AddModelError("", "Ocurrió un error al actualizar: " + ex.Message);
                CargarCategorias(elEquipment.CategoryId);
                return View("EditEquipment", elEquipment);
            }
        }

        private void CargarCategorias(int? seleccion = null)
        {
            using (var contexto = new Contexto())
            {
                ViewBag.Categorias = contexto.EqCategory
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName,
                        Selected = (seleccion.HasValue && seleccion.Value == c.CategoryId)
                    }).ToList();
            }
        }

        private void BorrarArchivoFisicoSilencioso(string urlVirtual)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(urlVirtual)) return;

                if (Uri.IsWellFormedUriString(urlVirtual, UriKind.Absolute)) return;

                var rutaFisica = Server.MapPath(urlVirtual);
                if (System.IO.File.Exists(rutaFisica))
                    System.IO.File.Delete(rutaFisica);
            }
            catch { }
        }

        public ActionResult Delete(int id)
        {
            return View();
        }

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

        public ActionResult Activar(int id)
        {
            var equipo = _ObtenerEqPorIDLN.Obtener(id);
            equipo.Status = 1; // Activo
            _equipmentLN.Actualizar(equipo);
            return RedirectToAction("ListarEquipment");
        }

        public ActionResult Inactivar(int id)
        {
            var equipo = _ObtenerEqPorIDLN.Obtener(id);
            equipo.Status = 3; // Inactivo
            _equipmentLN.Actualizar(equipo);
            return RedirectToAction("ListarEquipment");
        }
    }
}
