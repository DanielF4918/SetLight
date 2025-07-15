using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.AccesoADatos.Modelos
{
    public class TrazabilidadDA
    {
        public int EquipmentId { get; set; }
        public string TipoEvento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Persona { get; set; }
        public string RolPersona { get; set; }
    }
}

