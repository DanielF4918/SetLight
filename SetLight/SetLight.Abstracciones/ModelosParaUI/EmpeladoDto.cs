using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class EmpleadoDto
    {
        [Key]
        public int IdEmpleado { get; set; }


        public Guid? IdEmpleadoGuid { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(30)]
        [DisplayName("Nombre")]
        public string Nombre { get; set; }

        [Required]
        [StringLength(30)]
        [DisplayName("Apellido")]
        public string Apellido { get; set; }

        [Required]
        [StringLength(10)]
        [Phone]
        [DisplayName("Teléfono Celular")]
        public string TelefonoCelular { get; set; }

        [Required]
        [StringLength(50)]
        [EmailAddress]
        [DisplayName("Correo Electrónico")]
        public string CorreoElectronico { get; set; }

        [Required]
        [DisplayName("Rol del Empleado")]
        public string RolId { get; set; } 

        [DisplayName("Estado")]
        [Required]
        public bool Estado { get; set; } = true;
    }
}
