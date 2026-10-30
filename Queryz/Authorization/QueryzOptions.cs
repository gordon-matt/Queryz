namespace Queryz;

public sealed class QueryzOptions
{
    /// <summary>
    /// Every filter must allow the call. Leave this empty and Queryz stays closed,
    /// the same reason a Hangfire dashboard needs an explicit authorization filter.
    /// </summary>
    public List<IQueryzAuthorizationFilter> Authorization { get; } = [];
}
