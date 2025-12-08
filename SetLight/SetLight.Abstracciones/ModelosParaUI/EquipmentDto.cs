using System;
using System.ComponentModel.DataAnnotations;

namespace SetLight.Abstracciones.ModelosParaUI
{
    public class EquipmentDto
    {
        public int Faltantes { get; set; }

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

        [Display(Name = "Stock Disponible")]
        public int Stock { get; set; }

        public int CategoryId { get; set; }
        public int Status { get; set; }

        [Display(Name = "Disponibles")]
        public int Disponibles { get; set; }

        [Display(Name = "Alquilados")]
        public int Alquilados { get; set; }

        [Display(Name = "En Mantenimiento")]
        public int EnMantenimiento { get; set; }

        [Display(Name = "Equipo")]
        public string EquipoCompleto => $"{EquipmentName} {Brand} {Model}";

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

        [Display(Name = "Imagen (URL o ruta)")]
        [StringLength(500, ErrorMessage = "La ruta/URL de la imagen no debe superar 500 caracteres.")]
        public string ImageUrl { get; set; } 

        [Display(Name = "Tiene imagen")]
        public bool TieneImagen => !string.IsNullOrWhiteSpace(ImageUrl);
    }

    public class EquipmentFiltroDto
    {
        public string Nombre { get; set; }
        public int? CategoriaId { get; set; }
        public int? Estado { get; set; }
    }

}
