using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SetLight.AccesoADatos.Modelos
{
    [Table("Empleado")]
    public class EmpleadoDA
    {
        [Key]
        [Column("IdEmpleado")]
        public int IdEmpleado { get; set; }

        [Column("IdEmpleadoGuid")]
        public Guid? IdEmpleadoGuid { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; }

        [Column("Apellido")]
        public string Apellido { get; set; }

        [Column("TelefonoCelular")]
        public string TelefonoCelular { get; set; }

        [Column("CorreoElectronico")]
        public string CorreoElectronico { get; set; }

        [Column("RolId")]
        public string RolId { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; }

        [Column("Cedula")]
        public string Cedula { get; set; }

        [Column("ContactoEmergenciaNombre")]
        public string ContactoEmergenciaNombre { get; set; }

        [Column("ContactoEmergenciaTelefono")]
        public string ContactoEmergenciaTelefono { get; set; }

        [Column("ContactoEmergenciaParentesco")]
        public string ContactoEmergenciaParentesco { get; set; }

        [Column("TipoSangre")]
        public string TipoSangre { get; set; }

        [Column("Alergias")]
        public string Alergias { get; set; }

        [Column("InfoMedica")]
        public string InfoMedica { get; set; }

        // 🆕 Nueva columna para la foto de perfil
        [Column("FotoPerfil")]
        public string FotoPerfil { get; set; }
    }
}
