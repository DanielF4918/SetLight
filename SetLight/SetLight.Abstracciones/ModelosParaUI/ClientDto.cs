using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class ClientDto
    {
        public int ClientId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El teléfono personal es obligatorio.")]
        [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
        [Display(Name = "Teléfono personal")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Display(Name = "Estado")]
        public int Status { get; set; }

        [Display(Name = "Estado")]
        public string EstadoEnTexto
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Activo";
                    case 2: return "Agotado";
                    case 3: return "Inactivo";
                    default: return "Desconocido";
                }
            }
        }

        [Display(Name = "Nombre de la empresa")]
        public string EmpresaNombre { get; set; }

        [Display(Name = "Teléfono de la empresa")]
        public string EmpresaTelefono { get; set; }
    }
}
