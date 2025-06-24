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
        [Display(Name = "Nombre")]
        public string FirstName { get; set; }
        [Display(Name = "Apellido")]
        public string LastName { get; set; }
        [Display(Name = "Teléfono")]
        public string Phone { get; set; }
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
    }
}


