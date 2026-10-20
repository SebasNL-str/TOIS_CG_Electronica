using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models;

public class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Contrasena { get; set; } = string.Empty;

    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;
}