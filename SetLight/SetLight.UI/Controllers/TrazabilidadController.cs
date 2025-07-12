using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Trazabilidad;
using SetLight.LogicaDeNegocio.Trazabilidad.ObtenerTrazabilidadPorEquipo;

namespace SetLight.UI.Controllers
{
    public class TrazabilidadController : Controller
    {
        private readonly ITrazabilidadLN _trazabilidadLN;

        public TrazabilidadController()
        {
            _trazabilidadLN = new ObtenerTrazabilidadPorEquipoLN();
        }

        // GET: /Trazabilidad/VerTrazabilidad?equipoId=1
        public ActionResult VerTrazabilidad(int equipoId)
        {
            var trazabilidad = _trazabilidadLN.Ejecutar(equipoId);
            ViewBag.EquipoId = equipoId;
            return View("TrazabilidadPorEquipo", trazabilidad);
        }
    }
}
