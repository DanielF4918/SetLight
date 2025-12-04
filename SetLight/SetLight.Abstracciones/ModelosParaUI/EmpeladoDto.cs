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

        [DisplayName("Rol")]
        public string RolNombre { get; set; }

        [Required]
        [DisplayName("Estado")]
        public bool Estado { get; set; } = true;

        [StringLength(20)]
        [DisplayName("Número de Cédula")]
        public string Cedula { get; set; }

        [StringLength(60)]
        [DisplayName("Nombre del Contacto de Emergencia")]
        public string ContactoEmergenciaNombre { get; set; }

        [StringLength(20)]
        [DisplayName("Teléfono del Contacto de Emergencia")]
        public string ContactoEmergenciaTelefono { get; set; }

        [StringLength(30)]
        [DisplayName("Parentesco del Contacto de Emergencia")]
        public string ContactoEmergenciaParentesco { get; set; }

        [StringLength(3)]
        [DisplayName("Tipo de Sangre")]
        public string TipoSangre { get; set; }

        [StringLength(500)]
        [DisplayName("Alergias")]
        public string Alergias { get; set; }

        [StringLength(1000)]
        [DisplayName("Información Médica")]
        public string InfoMedica { get; set; }

        [StringLength(500)]
        [DisplayName("Foto de Perfil")]
        public string FotoPerfil { get; set; }

        // 🔐 Campo solo para confirmar cambios críticos
 
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña de administrador")]
        public string AdminPassword { get; set; }

    }
}
