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
        if (context.User.HasClaim("Permiso", requirement.Permiso))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}