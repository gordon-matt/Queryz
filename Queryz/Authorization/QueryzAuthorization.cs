using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Queryz;

/// <summary>
/// Applies the host's filters, then the group's roles and the report blacklist.
/// </summary>
public sealed class QueryzAuthorization(
    IHttpContextAccessor httpContextAccessor,
    AuthenticationStateProvider authenticationState,
    RoleManager<ApplicationRole> roles,
    QueryzOptions options) : IQueryzAuthorization
{
    private ClaimsPrincipal? user;
    private HashSet<string>? allowedRoleIds;

    public async Task<bool> CanUseAsync()
    {
        var caller = await CallerAsync();
        return options.Authorization.Count > 0 && options.Authorization.All(filter => filter.Authorize(caller));
    }

    public async Task<bool> CanManageAsync()
    {
        var caller = await CallerAsync();
        return options.Authorization.Count > 0 && options.Authorization.All(filter => filter.CanManage(caller));
    }

    public async Task<bool> CanRunAsync(IReadOnlyCollection<string> groupRoleIds, IReadOnlyCollection<string> deniedUserIds)
    {
        if (await CanManageAsync())
        {
            return true;
        }

        if (!await CanUseAsync())
        {
            return false;
        }

        var caller = await CallerAsync();
        string? userId = caller.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId) && deniedUserIds.Contains(userId, StringComparer.Ordinal))
        {
            return false;
        }

        if (groupRoleIds.Count == 0)
        {
            return false;
        }

        var allowed = await AllowedRoleIdsAsync();
        return groupRoleIds.Any(roleId => allowed.Contains(roleId));
    }

    private async Task<HashSet<string>> AllowedRoleIdsAsync()
    {
        if (allowedRoleIds is not null)
        {
            return allowedRoleIds;
        }

        var caller = await CallerAsync();
        var known = await roles.Roles.ToListAsync();
        allowedRoleIds = known
            .Where(role => role.Name is not null && caller.IsInRole(role.Name))
            .Select(role => role.Id)
            .ToHashSet(StringComparer.Ordinal);
        return allowedRoleIds;
    }

    private async Task<ClaimsPrincipal> CallerAsync()
    {
        if (user is not null)
        {
            return user;
        }

        var httpUser = httpContextAccessor.HttpContext?.User;
        if (httpUser?.Identity is not null)
        {
            user = httpUser;
            return user;
        }

        user = (await authenticationState.GetAuthenticationStateAsync()).User;
        return user;
    }
}
