using System.Security.Claims;

namespace Queryz;

/// <summary>
/// Host-supplied gate for Queryz, in the same role as Hangfire's <c>IDashboardAuthorizationFilter</c>.
/// </summary>
/// <remarks>
/// Pass instances when registering the library:
/// <c>options.Authorization.Add(new AdministratorQueryzAuthorizationFilter())</c>.
/// An empty list denies everyone. Blazor circuits do not keep an <c>HttpContext</c>, so the
/// filter receives the caller rather than a dashboard context.
/// Who may run one report is separate: the group's roles, then that report's blacklist.
/// </remarks>
public interface IQueryzAuthorizationFilter
{
    /// <summary>Whether this person may open Queryz and run reports they are allowed to see.</summary>
    bool Authorize(ClaimsPrincipal user);

    /// <summary>Whether this person may create and edit reports, groups, data sources, and enumerations.</summary>
    bool CanManage(ClaimsPrincipal user);
}
