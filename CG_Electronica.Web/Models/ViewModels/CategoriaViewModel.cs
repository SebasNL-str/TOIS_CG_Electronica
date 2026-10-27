using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models.ViewModels;

public class CategoriaViewModel
{
    [HiddenInput]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(150, ErrorMessage = "Máximo {1} caracteres.")]
    [Remote(action: "VerificarNombreCategoria", controller: "Categorias", AdditionalFields = nameof(IdCategoria), HttpMethod = "GET")]
    [Display(Name = "Nombre de Categoría")]
    public string NombreCategoria { get; set; } = null!;

    // NUEVO: Estado
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Activo";
}
