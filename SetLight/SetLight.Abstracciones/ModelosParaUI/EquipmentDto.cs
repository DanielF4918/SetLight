using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class EquipmentDto
    {
        public int EquipmentId { get; set; }

        [Display(Name = "Nombre del Equipo")]
        public string EquipmentName { get; set; }

        [Display(Name = "Marca")]
        public string Brand { get; set; }

        [Display(Name = "Modelo")]
        public string Model { get; set; }

        [Display(Name = "Número de Serie")]
        public string SerialNumber { get; set; }

        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Display(Name = "Valor de Alquiler")]
        public decimal RentalValue { get; set; }

        [Display(Name = "Categoría")]
        public string CategoriaNombre { get; set; }

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

        public int CategoryId { get; set; }
        public int Status { get; set; }


    }

    public class EquipmentFiltroDto
    {
        public string Nombre { get; set; }
        public int? CategoriaId { get; set; }
        public int? Estado { get; set; }
    }

}
