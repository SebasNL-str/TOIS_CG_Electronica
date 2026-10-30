using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models;

public class Rol
{
    [Key]
    public int IdRol { get; set; }

    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(100)]
    public string NombreRol { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    public string Estado { get; set; } = "Activo";
}
