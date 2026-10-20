using System.ComponentModel.DataAnnotations;

namespace CG_Electronica.Web.Models;

public class Permiso
{
    public int IdPermiso { get; set; }

    [Required(ErrorMessage = "El nombre del permiso es obligatorio.")]
    [StringLength(100)]
    public string NombrePermiso { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Descripcion { get; set; }

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}