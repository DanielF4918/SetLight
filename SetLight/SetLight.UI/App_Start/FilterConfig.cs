using System.Web;
using System.Web.Mvc;
using SetLight.UI.Filters;

namespace SetLight.UI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new VerificarEmpleadoActivoAttribute());
        }
    }
}
