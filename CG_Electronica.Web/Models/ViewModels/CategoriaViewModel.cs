using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models.ViewModels;

public class CategoriaViewModel
{
    [HiddenInput]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Categoría")]
    public string NombreCategoria { get; set; } = string.Empty;
}
