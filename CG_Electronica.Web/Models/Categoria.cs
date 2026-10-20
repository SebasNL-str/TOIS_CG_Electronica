using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models;

public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(150)]
    public string NombreCategoria { get; set; } = string.Empty;
}