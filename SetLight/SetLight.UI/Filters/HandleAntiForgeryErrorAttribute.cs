using System.Web.Mvc;

namespace SetLight.UI.Filters
{
    public class HandleAntiForgeryErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is HttpAntiForgeryException)
            {
                // Evita la pantalla amarilla
                filterContext.ExceptionHandled = true;

                // Mensaje friendly
                filterContext.Controller.TempData["Error"] =
                    "Tu sesión cambió (abriste otra cuenta en otra pestaña). " +
                    "Por seguridad, refrescá la página e intentá de nuevo.";

                // Redirigimos a una página segura (ej. Login o Home)
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Account", action = "Login" }
                    )
                );
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
}
