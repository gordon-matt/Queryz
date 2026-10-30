using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;

namespace Queryz;

/// <summary>
/// Authenticated people may run reports. Administrators may set them up.
/// </summary>
/// <remarks>
/// Register it explicitly:
/// <c>options.Authorization.Add(new AdministratorQueryzAuthorizationFilter())</c>.
/// </remarks>
public sealed class AdministratorQueryzAuthorizationFilter(string role = WellKnownRoles.Administrator)
    : IQueryzAuthorizationFilter
{
    public bool Authorize(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true;

    public bool CanManage(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true
        && user.IsInRole(role);
}
