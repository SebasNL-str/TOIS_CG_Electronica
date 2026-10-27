using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models;

public class Marca
{
    [Key]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(150)]
    public string NombreMarca { get; set; } = string.Empty;


    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = "Activo";
}