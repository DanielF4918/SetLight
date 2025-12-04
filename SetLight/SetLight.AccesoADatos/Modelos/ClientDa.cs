using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetLight.AccesoADatos.Modelos
{
    [Table("Clients")]
    public class ClientDa
    {
        [Key]
        [Column("ClientId")]
        public int ClientId { get; set; }

        [Column("FirstName")]
        public string FirstName { get; set; }

        [Column("LastName")]
        public string LastName { get; set; }

        [Column("Phone")]
        public string Phone { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("Status")]
        public int Status { get; set; }


        [Column("EmpresaNombre")]
        [StringLength(150)]
        public string EmpresaNombre { get; set; }

        [Column("EmpresaTelefono")]
        [StringLength(25)]
        public string EmpresaTelefono { get; set; }
    }
}
