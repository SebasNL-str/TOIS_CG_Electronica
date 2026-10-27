using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }
}
