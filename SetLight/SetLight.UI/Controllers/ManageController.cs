using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SetLight.UI.Models;
using System.Security.Claims;
using SetLight.LogicaDeNegocio.Empleado;
using SetLight.Abstracciones.ViewModels;
using SetLight.Abstracciones.LogicaDeNegocio.Empleado;
using SetLight.LogicaDeNegocio.Empleado.ObtenerEmpleadoPorID;
using SetLight.Abstracciones.ModelosParaUI;
using SetLight.AccesoADatos;

namespace SetLight.UI.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IObtenerEmpleadoPorIDLN _empleadoLN;

        public ManageController()
        {
            _empleadoLN = new ObtenerEmpleadoPorIDLN();
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, IObtenerEmpleadoPorIDLN empleadoLN)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _empleadoLN = empleadoLN;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set { _userManager = value; }
        }





        // GET: /Manage/Index
        [HttpGet]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId(); 

            EmpleadoDto model;

            using (var contexto = new Contexto())
            {
                model = contexto.Empleado
                    .Where(e => e.IdEmpleadoGuid.ToString() == userId) 
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

            if (model == null)
                return HttpNotFound();

            return View("Index", model);
        }








        //


        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Se ha quitado el inicio de sesión externo."
                : message == ManageMessageId.Error ? "Se ha producido un error."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes()
                .Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider))
                .ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            base.Dispose(disposing);
        }

        #region Aplicaciones auxiliares
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return user != null && user.PasswordHash != null;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return user != null && user.PhoneNumber != null;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
        #endregion
    }
}
