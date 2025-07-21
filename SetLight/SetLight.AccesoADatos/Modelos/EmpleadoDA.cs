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
    }
}
