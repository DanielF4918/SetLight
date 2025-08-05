using System.Web.Mvc;
using SetLight.Abstracciones.LogicaDeNegocio.Trazabilidad;
using SetLight.LogicaDeNegocio.Trazabilidad.ObtenerTrazabilidadPorEquipo;
using SetLight.LogicaDeNegocio.Trazabilidad;
using System.IO;

namespace SetLight.UI.Controllers
{
    [Authorize(Roles = "Administrador,Colaborador")]
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
        public ActionResult DescargarTrazabilidad(int equipoId)
        {
            var trazabilidad = _trazabilidadLN.Ejecutar(equipoId);
            var ruta = new TrazabilidadPdfService().Generar(equipoId, trazabilidad);
            return File(ruta, "application/pdf", Path.GetFileName(ruta));
        }

    }
}
