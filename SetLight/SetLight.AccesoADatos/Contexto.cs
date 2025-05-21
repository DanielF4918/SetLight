using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetLight.AccesoADatos.Modelos;

namespace SetLight.AccesoADatos
{
    public class Contexto: DbContext
    {
        public Contexto() : base("name=Contexto")
        {

        }

        public DbSet<EquipmentDA> Equipment { get; set; }
    }
}
