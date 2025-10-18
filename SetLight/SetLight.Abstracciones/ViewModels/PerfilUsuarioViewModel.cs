using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.ViewModels
{
    public class PerfilUsuarioViewModel
    {
        public int IdEmpleado { get; set; }
        public Guid? IdEmpleadoGuid { get; set; }

        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [DisplayName("Apellido")]
        public string Apellido { get; set; }

        [DisplayName("Correo Electrónico")]
        public string CorreoElectronico { get; set; }

        [DisplayName("Teléfono Celular")]
        public string TelefonoCelular { get; set; }

        [DisplayName("Número de Cédula")]
        public string Cedula { get; set; }

        [DisplayName("Nombre del Contacto de Emergencia")]
        public string ContactoEmergenciaNombre { get; set; }

        [DisplayName("Teléfono del Contacto de Emergencia")]
        public string ContactoEmergenciaTelefono { get; set; }

        [DisplayName("Parentesco del Contacto de Emergencia")]
        public string ContactoEmergenciaParentesco { get; set; }

        [DisplayName("Tipo de Sangre")]
        public string TipoSangre { get; set; }

        [DisplayName("Alergias")]
        public string Alergias { get; set; }

        [DisplayName("Información Médica")]
        public string InfoMedica { get; set; }


        public bool TienePassword { get; set; }
        public string NumeroTelefono { get; set; }
        public bool TwoFactor { get; set; }
        public bool NavegadorRecordado { get; set; }


        public string MensajeInformativo { get; set; }
    }
}

