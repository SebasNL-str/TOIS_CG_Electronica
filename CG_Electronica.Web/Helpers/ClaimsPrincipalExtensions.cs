using System.Security.Claims;

namespace CG_Electronica.Web.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static bool TienePermiso(this ClaimsPrincipal user, string permiso)
    {
        return user.Claims.Any(c =>
            c.Type == "Permiso" && c.Value == permiso);
    }

    public static string? NombreUsuario(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Name)?.Value;

    public static string? NombreCompleto(this ClaimsPrincipal user)
        => user.FindFirst("NombreCompleto")?.Value;

    public static string? Rol(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value;
}
