using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using SetLight.Abstracciones.LogicaDeNegocio.Trazabilidad;
using SetLight.LogicaDeNegocio.Trazabilidad;
using SetLight.LogicaDeNegocio.Trazabilidad.ObtenerTrazabilidadPorEquipo;

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
            int pageNumber = 1;
            int pageSize = 12;

            // Página
            var pageRaw = Request.QueryString["page"];
            if (!string.IsNullOrWhiteSpace(pageRaw))
            {
                int.TryParse(pageRaw, out pageNumber);
                if (pageNumber <= 0) pageNumber = 1;
            }

            // Filtro DESDE
            DateTime? desde = null;
            var desdeRaw = Request.QueryString["desde"];
            if (!string.IsNullOrWhiteSpace(desdeRaw))
            {
                if (DateTime.TryParse(desdeRaw, out DateTime tmpDesde))
                    desde = tmpDesde.Date;
            }

            // Filtro HASTA
            DateTime? hasta = null;
            var hastaRaw = Request.QueryString["hasta"];
            if (!string.IsNullOrWhiteSpace(hastaRaw))
            {
                if (DateTime.TryParse(hastaRaw, out DateTime tmpHasta))
                    hasta = tmpHasta.Date;
            }

            // Traer toda la trazabilidad del equipo
            var trazabilidad = _trazabilidadLN.Ejecutar(equipoId);

            // 🔹 Nombre del equipo (lo tomamos ANTES de filtrar para no perderlo)
            var primero = trazabilidad.FirstOrDefault();

            ViewBag.EquipoNombre = primero != null
                ? (!string.IsNullOrWhiteSpace(primero.EquipmentNombre)
                    ? primero.EquipmentNombre
                    : "Sin nombre configurado")
                : "Sin registros";

            // Aplicar filtros de fecha
            if (desde.HasValue)
            {
                trazabilidad = trazabilidad
                    .Where(x =>
                        (x.FechaInicio.HasValue && x.FechaInicio.Value.Date >= desde.Value) ||
                        (x.FechaMantenimiento.HasValue && x.FechaMantenimiento.Value.Date >= desde.Value)
                    ).ToList();
            }

            if (hasta.HasValue)
            {
                trazabilidad = trazabilidad
                    .Where(x =>
                        (x.FechaFin.HasValue && x.FechaFin.Value.Date <= hasta.Value) ||
                        (x.FechaMantenimiento.HasValue && x.FechaMantenimiento.Value.Date <= hasta.Value)
                    ).ToList();
            }

            var paginado = trazabilidad.ToPagedList(pageNumber, pageSize);

            ViewBag.EquipoId = equipoId;
            ViewBag.FiltroDesde = desde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = hasta?.ToString("yyyy-MM-dd");

            return View("TrazabilidadPorEquipo", paginado);
        }



        // GET: /Trazabilidad/DescargarTrazabilidad?equipoId=1&desde=2025-12-01&hasta=2025-12-08
        public ActionResult DescargarTrazabilidad(int equipoId)
        {
            // Leer filtros desde el querystring (igual que en VerTrazabilidad)
            DateTime? desde = null;
            var desdeRaw = Request.QueryString["desde"];
            if (!string.IsNullOrWhiteSpace(desdeRaw))
            {
                if (DateTime.TryParse(desdeRaw, out DateTime tmpDesde))
                    desde = tmpDesde.Date;
            }

            DateTime? hasta = null;
            var hastaRaw = Request.QueryString["hasta"];
            if (!string.IsNullOrWhiteSpace(hastaRaw))
            {
                if (DateTime.TryParse(hastaRaw, out DateTime tmpHasta))
                    hasta = tmpHasta.Date;
            }

            var trazabilidad = _trazabilidadLN.Ejecutar(equipoId);

            // Aplicar los mismos filtros que en la vista
            if (desde.HasValue)
            {
                trazabilidad = trazabilidad
                    .Where(x =>
                        (x.FechaInicio.HasValue && x.FechaInicio.Value.Date >= desde.Value) ||
                        (x.FechaMantenimiento.HasValue && x.FechaMantenimiento.Value.Date >= desde.Value)
                    ).ToList();
            }

            if (hasta.HasValue)
            {
                trazabilidad = trazabilidad
                    .Where(x =>
                        (x.FechaFin.HasValue && x.FechaFin.Value.Date <= hasta.Value) ||
                        (x.FechaMantenimiento.HasValue && x.FechaMantenimiento.Value.Date <= hasta.Value)
                    ).ToList();
            }

            var ruta = new TrazabilidadPdfService().Generar(equipoId, trazabilidad);
            return File(ruta, "application/pdf", Path.GetFileName(ruta));
        }
    }
}
