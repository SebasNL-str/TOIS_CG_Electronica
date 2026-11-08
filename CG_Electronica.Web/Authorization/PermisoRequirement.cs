using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CG_Electronica.Web.Authorization;

public class PermisoRequirement : IAuthorizationRequirement
{
    public string Permiso { get; }
    public PermisoRequirement(string permiso) => Permiso = permiso;
}

public class PermisoHandler : AuthorizationHandler<PermisoRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermisoRequirement requirement)
    {
        var permisoRequerido = requirement.Permiso?.Trim();

        if (!string.IsNullOrEmpty(permisoRequerido))
        {
            // Búsqueda robusta para CUALQUIER ROL:
            // 1. Limpia espacios en blanco (.Trim()) tanto del claim como del requirement
            // 2. Compara ignorando mayúsculas y minúsculas (OrdinalIgnoreCase)
            bool tienePermiso = context.User.Claims.Any(c =>
                c.Type == "Permiso" &&
                string.Equals(c.Value.Trim(), permisoRequerido, StringComparison.OrdinalIgnoreCase));

            if (tienePermiso)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}