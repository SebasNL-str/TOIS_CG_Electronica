using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models.ViewModels;

public class MarcaViewModel
{
    [HiddenInput]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Marca")]
    public string NombreMarca { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(30)]
    [Display(Name = "Estado")]
    public string Estado { get; set; } = "Activo"; // Activo / Inactivo
}