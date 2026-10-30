namespace Queryz;

/// <summary>
/// What the current caller may do inside Queryz. Services use this so a Blazor event and an
/// export request follow the same rules. The host decides the rules with
/// <see cref="IQueryzAuthorizationFilter"/>.
/// </summary>
public interface IQueryzAuthorization
{
    Task<bool> CanUseAsync();

    Task<bool> CanManageAsync();

    /// <summary>
    /// A client may run a report when one of the group's roles is theirs and they are not on
    /// that report's blacklist. Someone who can manage the catalog can run every report.
    /// A group with no roles is visible only to people who can manage.
    /// </summary>
    Task<bool> CanRunAsync(IReadOnlyCollection<string> groupRoleIds, IReadOnlyCollection<string> deniedUserIds);
}
