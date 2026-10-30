using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Queryz.Infrastructure;

public sealed class QueryzNavigation(IReportGroupService groups) : IShellNavigation
{
    public string? SearchUrlFormat => "/?q={0}";

    public async Task<IReadOnlyList<ForgeNavSection>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        var listed = await groups.ListAsync(cancellationToken);
        var items = new List<ForgeNavItem>
        {
            new()
            {
                Title = "All reports",
                Href = "/",
                Icon = "library",
                MatchAll = true,
                ActivePaths = ["/", "/reports"],
                ActiveQuery = new Dictionary<string, string?> { ["group"] = null, ["q"] = null },
            },
        };

        if (listed.IsSuccess)
        {
            items.AddRange(listed.Value.Select(group => new ForgeNavItem
            {
                Title = group.Name,
                Href = $"/?group={Uri.EscapeDataString(group.Name)}",
                Icon = "folder",
                MatchAll = true,
                ActivePaths = ["/", "/reports"],
                ActiveQuery = new Dictionary<string, string?> { ["group"] = group.Name },
            }));
        }

        return
        [
            new ForgeNavSection
            {
                Items =
                [
                    new ForgeNavItem { Title = "New report", Href = "/reports/new", Icon = "plus" },
                    new ForgeNavItem { Title = "Data sources", Href = "/data-sources", Icon = "database" },
                    new ForgeNavItem { Title = "Groups", Href = "/groups", Icon = "folder" },
                    new ForgeNavItem { Title = "Enumerations", Href = "/enumerations", Icon = "list" },
                ],
            },
            new ForgeNavSection
            {
                Heading = "Groups",
                Items = items,
            },
            new ForgeNavSection
            {
                Heading = "Administration",
                Roles = WellKnownRoles.Administrator,
                Items =
                [
                    new ForgeNavItem
                    {
                        Title = "Logs",
                        Href = "/logvue",
                        Icon = "logs",
                        External = true,
                    },
                ],
            },
        ];
    }
}
