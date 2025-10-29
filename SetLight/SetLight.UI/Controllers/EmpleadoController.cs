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
        public async Task<ActionResult> CrearEmpleado(EmpleadoDto empleadoDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = ObtenerListaRoles(empleadoDto.RolId);
                return View(empleadoDto);
            }

            try
            {
                await _crearEmpleadoLN.Guardar(empleadoDto);
                TempData["Ok"] = "Empleado registrado correctamente.";
                return RedirectToAction("ListarEmpleado");
            }
            // EF6 suele envolver la SqlException dentro de DbUpdateException
            catch (DbUpdateException ex)
            {
                if (EsViolacionUnicidad(ex))
                {
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo registrar el empleado. Intente nuevamente.");
                }

                ViewBag.Roles = ObtenerListaRoles(empleadoDto.RolId);
                return View(empleadoDto);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627) // UNIQUE KEY violation
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                else
                    ModelState.AddModelError("", "No se pudo registrar el empleado. Intente nuevamente.");

                ViewBag.Roles = ObtenerListaRoles(empleadoDto.RolId);
                return View(empleadoDto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error inesperado: " + ex.Message);
                ViewBag.Roles = ObtenerListaRoles(empleadoDto.RolId);
                return View(empleadoDto);
            }
        }

        // Helper robusto para detectar violación de índice/constraint UNIQUE (2601/2627)
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
        public ActionResult Edit(EmpleadoDto model)
        {
            if (string.IsNullOrWhiteSpace(model.RolId))
                ModelState.AddModelError(nameof(model.RolId), "Debe seleccionar un rol.");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = ObtenerListaRoles(model.RolId);
                ViewBag.Estados = new[]
                {
            new SelectListItem { Value = bool.TrueString,  Text = "Activo",   Selected = model.Estado },
            new SelectListItem { Value = bool.FalseString, Text = "Inactivo", Selected = !model.Estado }
        };
                return View("Edit", model);
            }

            try
            {
                int filasAfectadas = _editarEmpleadoAD.Editar(model);

                if (filasAfectadas <= 0)
                {
                    ModelState.AddModelError("", "No se pudo actualizar el empleado.");
                }
                else
                {
                    TempData["Ok"] = "Empleado actualizado correctamente.";
                    return RedirectToAction("ListarEmpleado");
                }
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.InnerException as SqlException;
                if (inner != null && (inner.Number == 2601 || inner.Number == 2627))
                {
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                }
                else
                {
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el empleado.");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                    ModelState.AddModelError("", "Ya existe un empleado con la misma cédula o correo electrónico.");
                else
                    ModelState.AddModelError("", "Ocurrió un error al actualizar el empleado.");
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al actualizar el empleado.");
            }

            // Recargar combos si hay error y vuelve a la vista
            ViewBag.Roles = ObtenerListaRoles(model.RolId);
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
