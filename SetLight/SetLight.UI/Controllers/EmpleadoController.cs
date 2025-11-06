using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Empleado;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.LogicaDeNegocio.Empleado.CrearEmpleado;
using SetLight.LogicaDeNegocio.Empleado.ListarEmpleado;
using SetLight.UI.Models; 
using Microsoft.AspNet.Identity.EntityFramework;
using SetLight.LogicaDeNegocio.Empleado.ObtenerEmpleadoPorID;
using System.Runtime.Remoting.Contexts;
using SetLight.AccesoADatos;
using SetLight.Abstracciones.AccesoADatos.Empleado;
using SetLight.AccesoADatos.Empleado.EditarEmpleado;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace SetLight.UI.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EmpleadoController : Controller
    {
        private  IListarEmpleadoLN _listarEmpleadoLN;
        private  ICrearEmpleadoLN _crearEmpleadoLN;
        private  ApplicationDbContext _contexto;
        private IObtenerEmpleadoPorIDLN _obtenerEmpleadoPorIDLN;
        private IEditarEmpleadoAD _editarEmpleadoAD;


        public EmpleadoController()
        {
            _listarEmpleadoLN = new ListarEmpleadoLN();
            _crearEmpleadoLN = new CrearEmpleadoLN();
            _contexto = new ApplicationDbContext();
            _obtenerEmpleadoPorIDLN = new ObtenerEmpleadoPorIDLN();
            _editarEmpleadoAD = new EditarEmpleadoAD();
        }


        private string ObtenerUserIdPorCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return null;

            return _contexto.Users
                .Where(u => u.Email == correo)
                .Select(u => u.Id)
                .FirstOrDefault();
        }

        private string ObtenerNombreRolPorId(string rolId)
        {
            if (string.IsNullOrWhiteSpace(rolId)) return null;

            return _contexto.Roles
                .Where(r => r.Id == rolId)
                .Select(r => r.Name)
                .FirstOrDefault();
        }

        public ApplicationUserManager UserManager =>
    HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        public ApplicationSignInManager SignInManager =>
            HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

        // GET: Empleado
        public ActionResult ListarEmpleado()
        {
            List<EmpleadoDto> listaEmpleados = _listarEmpleadoLN.Obtener();
            return View(listaEmpleados);
        }

        // GET: Empleado/Create
        public ActionResult CrearEmpleado()
        {
            ViewBag.Roles = ObtenerListaRoles();
            return View();
        }

        // POST: Empleado/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CrearEmpleado(EmpleadoDto empleadoDto, HttpPostedFileBase foto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = ObtenerListaRoles(empleadoDto.RolId);
                return View(empleadoDto);
            }

            try
            {
                // 🆕 Guardar foto si se sube
                if (foto != null && foto.ContentLength > 0)
                {
                    var path = Server.MapPath("~/Content/img/empleados/");
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);

                    var fileName = $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(foto.FileName)}";
                    var fullPath = System.IO.Path.Combine(path, fileName);
                    foto.SaveAs(fullPath);

                    // Guardamos la ruta relativa
                    empleadoDto.FotoPerfil = $"/Content/img/empleados/{fileName}";
                }

                await _crearEmpleadoLN.Guardar(empleadoDto);
                TempData["Ok"] = "Empleado registrado correctamente.";
                return RedirectToAction("ListarEmpleado");
            }
            catch (DbUpdateException ex)
            {
                if (EsViolacionUnicidad(ex))
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                else
                    ModelState.AddModelError("", "No se pudo registrar el empleado. Intente nuevamente.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                else
                    ModelState.AddModelError("", "No se pudo registrar el empleado. Intente nuevamente.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error inesperado: " + ex.Message);
            }

            ViewBag.Roles = ObtenerListaRoles(empleadoDto.RolId);
            return View(empleadoDto);
        }


        private static bool EsViolacionUnicidad(Exception ex)
        {
            while (ex != null)
            {
                var sqlEx = ex as SqlException;
                if (sqlEx != null && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                    return true;

                ex = ex.InnerException;
            }
            return false;
        }

        private IEnumerable<SelectListItem> ObtenerListaRoles(string rolSeleccionado = null)
        {
            return _contexto.Roles.Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name,
                Selected = r.Id == rolSeleccionado
            }).ToList();
        }



        // GET: Empleado/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            EmpleadoDto model;

            using (var contexto = new Contexto())
            {
                model = contexto.Empleado
                    .Where(e => e.IdEmpleado == id)
                    .Select(e => new EmpleadoDto
                    {
                        IdEmpleado = e.IdEmpleado,
                        IdEmpleadoGuid = e.IdEmpleadoGuid,
                        Nombre = e.Nombre,
                        Apellido = e.Apellido,
                        TelefonoCelular = e.TelefonoCelular,
                        CorreoElectronico = e.CorreoElectronico,
                        RolId = e.RolId,
                        Estado = e.Estado,

                        Cedula = e.Cedula,
                        ContactoEmergenciaNombre = e.ContactoEmergenciaNombre,
                        ContactoEmergenciaTelefono = e.ContactoEmergenciaTelefono,
                        ContactoEmergenciaParentesco = e.ContactoEmergenciaParentesco,
                        TipoSangre = e.TipoSangre,
                        Alergias = e.Alergias,
                        InfoMedica = e.InfoMedica
                    })
                    .FirstOrDefault();
            }

            if (model == null) return HttpNotFound();

            return View("Details", model);
        }


        // GET: Empleado/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            EmpleadoDto model;

            using (var contexto = new Contexto())
            {
                model = contexto.Empleado
                    .Where(e => e.IdEmpleado == id)
                    .Select(e => new EmpleadoDto
                    {
                        IdEmpleado = e.IdEmpleado,
                        IdEmpleadoGuid = e.IdEmpleadoGuid,

                        Nombre = e.Nombre,
                        Apellido = e.Apellido,
                        TelefonoCelular = e.TelefonoCelular,
                        CorreoElectronico = e.CorreoElectronico,
                        RolId = e.RolId,
                        Estado = e.Estado,
                        Cedula = e.Cedula,
                        ContactoEmergenciaNombre = e.ContactoEmergenciaNombre,
                        ContactoEmergenciaTelefono = e.ContactoEmergenciaTelefono,
                        ContactoEmergenciaParentesco = e.ContactoEmergenciaParentesco,
                        TipoSangre = e.TipoSangre,
                        Alergias = e.Alergias,
                        InfoMedica = e.InfoMedica
                    })
                    .FirstOrDefault();
            }

            if (model == null) return HttpNotFound();

            ViewBag.Roles = ObtenerListaRoles(model.RolId);
            ViewBag.Roles = ObtenerListaRoles(model.RolId);

            ViewBag.Estados = new[]
            {
        new SelectListItem { Value = bool.TrueString,  Text = "Activo",   Selected = model.Estado },
        new SelectListItem { Value = bool.FalseString, Text = "Inactivo", Selected = !model.Estado }
    };

            return View("Edit", model);
        }


        // POST: Empleado/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EmpleadoDto model, HttpPostedFileBase nuevaFoto)
        {
            if (string.IsNullOrWhiteSpace(model.RolId))
                ModelState.AddModelError(nameof(model.RolId), "Debe seleccionar un rol.");

            EmpleadoDto actual;
            using (var ctx = new Contexto())
            {
                actual = ctx.Empleado
                    .Where(e => e.IdEmpleado == model.IdEmpleado)
                    .Select(e => new EmpleadoDto
                    {
                        IdEmpleado = e.IdEmpleado,
                        Nombre = e.Nombre,
                        Apellido = e.Apellido,
                        TelefonoCelular = e.TelefonoCelular,
                        CorreoElectronico = e.CorreoElectronico,
                        RolId = e.RolId,
                        Estado = e.Estado,
                        Cedula = e.Cedula,
                        ContactoEmergenciaNombre = e.ContactoEmergenciaNombre,
                        ContactoEmergenciaTelefono = e.ContactoEmergenciaTelefono,
                        ContactoEmergenciaParentesco = e.ContactoEmergenciaParentesco,
                        TipoSangre = e.TipoSangre,
                        Alergias = e.Alergias,
                        InfoMedica = e.InfoMedica,
                        FotoPerfil = e.FotoPerfil
                    })
                    .FirstOrDefault();
            }

            if (actual == null)
            {
                ModelState.AddModelError("", "El empleado no existe.");
            }

            var correoCambio = actual != null && !string.Equals(actual.CorreoElectronico?.Trim(), model.CorreoElectronico?.Trim(), StringComparison.OrdinalIgnoreCase);
            if (actual != null && correoCambio)
            {
                var correoNuevo = model.CorreoElectronico?.Trim();

                using (var ctx = new Contexto())
                {
                    var existeEnEmpleado = ctx.Empleado
                        .Any(e => e.CorreoElectronico == correoNuevo && e.IdEmpleado != model.IdEmpleado);
                    if (existeEnEmpleado)
                        ModelState.AddModelError(nameof(model.CorreoElectronico), "Ya existe un empleado con ese correo.");
                }

                var existeEnIdentity = _contexto.Users
                    .Any(u => u.Email == correoNuevo && u.Email != actual.CorreoElectronico);
                if (existeEnIdentity)
                    ModelState.AddModelError(nameof(model.CorreoElectronico), "Ese correo ya está registrado en la cuenta de acceso.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _contexto.Roles.Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name,
                    Selected = r.Id == model.RolId
                }).ToList();

                ViewBag.Estados = new[]
                {
            new SelectListItem { Value = bool.TrueString,  Text = "Activo",   Selected = model.Estado },
            new SelectListItem { Value = bool.FalseString, Text = "Inactivo", Selected = !model.Estado }
        };

                return View("Edit", model);
            }

            try
            {
                if (nuevaFoto != null && nuevaFoto.ContentLength > 0)
                {
                    var path = Server.MapPath("~/Content/img/empleados/");
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);

                    var fileName = $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(nuevaFoto.FileName)}";
                    var fullPath = System.IO.Path.Combine(path, fileName);
                    nuevaFoto.SaveAs(fullPath);

                    model.FotoPerfil = $"/Content/img/empleados/{fileName}";
                }
                else
                {
                    model.FotoPerfil = actual?.FotoPerfil;
                }

                var filas = _editarEmpleadoAD.Editar(model);
                if (filas <= 0)
                {
                    ModelState.AddModelError("", "No se pudo actualizar el empleado.");
                }
                else
                {
                    if (correoCambio)
                    {
                        var user = await UserManager.FindByEmailAsync(actual.CorreoElectronico);
                        if (user != null)
                        {
                            user.Email = model.CorreoElectronico.Trim();
                            user.UserName = model.CorreoElectronico.Trim();

                            var upd = await UserManager.UpdateAsync(user);
                            if (!upd.Succeeded)
                            {
                                ModelState.AddModelError("", "No se pudo actualizar el correo en Identity: " + string.Join("; ", upd.Errors));
                            }
                            else
                            {
                                await UserManager.UpdateSecurityStampAsync(user.Id);

                                if (User.Identity.GetUserId() == user.Id)
                                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Advertencia: no se encontró la cuenta de acceso en Identity para el correo anterior.");
                        }
                    }

                    var userIdIdentity = _contexto.Users
                        .Where(u => u.Email == model.CorreoElectronico)
                        .Select(u => u.Id)
                        .FirstOrDefault();

                    var rolNombre = _contexto.Roles
                        .Where(r => r.Id == model.RolId)
                        .Select(r => r.Name)
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(userIdIdentity) && !string.IsNullOrWhiteSpace(rolNombre))
                    {
                        var rolesActuales = await UserManager.GetRolesAsync(userIdIdentity);
                        if (rolesActuales.Any())
                            await UserManager.RemoveFromRolesAsync(userIdIdentity, rolesActuales.ToArray());

                        var add = await UserManager.AddToRoleAsync(userIdIdentity, rolNombre);
                        if (!add.Succeeded)
                        {
                            ModelState.AddModelError("", "No se pudo actualizar el rol en Identity: " + string.Join("; ", add.Errors));
                        }
                        else
                        {
                            await UserManager.UpdateSecurityStampAsync(userIdIdentity);

                            if (User.Identity.GetUserId() == userIdIdentity)
                            {
                                var u = await UserManager.FindByIdAsync(userIdIdentity);
                                await SignInManager.SignInAsync(u, isPersistent: false, rememberBrowser: false);
                            }
                        }
                    }

                    if (!ModelState.Values.SelectMany(v => v.Errors).Any())
                    {
                        TempData["Ok"] = "Empleado actualizado correctamente.";
                        return RedirectToAction("ListarEmpleado");
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.InnerException as SqlException;
                if (inner != null && (inner.Number == 2601 || inner.Number == 2627))
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                else
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el empleado.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                else
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el empleado.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error inesperado: " + ex.Message);
            }

            ViewBag.Roles = _contexto.Roles.Select(r => new SelectListItem
            {
                Value = r.Id,
                Text = r.Name,
                Selected = r.Id == model.RolId
            }).ToList();

            ViewBag.Estados = new[]
            {
        new SelectListItem { Value = bool.TrueString,  Text = "Activo",   Selected = model.Estado },
        new SelectListItem { Value = bool.FalseString, Text = "Inactivo", Selected = !model.Estado }
    };

            return View("Edit", model);
        }








        // GET: Empleado/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Empleado/Delete/5
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


        // GET: Empleado/Activar/5
        public ActionResult Activar(int id)
        {
            using (var ctx = new Contexto())
            {
                var emp = ctx.Empleado.FirstOrDefault(e => e.IdEmpleado == id);
                if (emp == null) return HttpNotFound();

                emp.Estado = true; // Activo
                ctx.SaveChanges();
                TempData["Ok"] = "Empleado activado.";
            }

            return RedirectToAction("ListarEmpleado");
        }

        // GET: Empleado/Inactivar/5
        public ActionResult Inactivar(int id)
        {
            using (var ctx = new Contexto())
            {
                var emp = ctx.Empleado.FirstOrDefault(e => e.IdEmpleado == id);
                if (emp == null) return HttpNotFound();

                emp.Estado = false; // Inactivo
                ctx.SaveChanges();
                TempData["Ok"] = "Empleado inactivado.";
            }

            return RedirectToAction("ListarEmpleado");
        }



    }


}
