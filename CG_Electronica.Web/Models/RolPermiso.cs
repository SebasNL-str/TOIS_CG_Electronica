namespace CG_Electronica.Web.Models;

public class RolPermiso
{
    public int IdRol { get; set; }
    public int IdPermiso { get; set; }

    public Rol Rol { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
}