using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SetLight.AccesoADatos;

namespace SetLight.UI.Filters
{
    public class VerificarEmpleadoActivoAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // Primero que haga la lógica normal de [Authorize]
            base.OnAuthorization(filterContext);

            var user = filterContext.HttpContext.User;

            // Si no está autenticado, no hacemos nada
            if (user == null || !user.Identity.IsAuthenticated)
                return;

            // Correo del usuario logueado (es el UserName que usás para login)
            var email = user.Identity.Name;

            using (var ctx = new Contexto())
            {
                var empleado = ctx.Empleado
                    .FirstOrDefault(e => e.CorreoElectronico == email);

                // Si no existe empleado o sigue activo, no hacemos nada
                if (empleado == null || empleado.Estado)
                    return;

                // ⚠️ Si el empleado está inactivo → cerrar sesión y redirigir a Login
                var owin = filterContext.HttpContext.GetOwinContext();
                owin.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                // Mensaje que se mostrará en la pantalla de login
                filterContext.Controller.TempData["CuentaDesactivada"] =
                    "Tu cuenta ha sido desactivada. Contacta al administrador para más información.";

                // Redirigir al Login
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new { controller = "Account", action = "Login" }
                    )
                );
            }
        }
    }
}
