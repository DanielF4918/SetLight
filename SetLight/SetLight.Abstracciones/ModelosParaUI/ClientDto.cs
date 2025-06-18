using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class ClientDto
    {
        public int ClientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
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


