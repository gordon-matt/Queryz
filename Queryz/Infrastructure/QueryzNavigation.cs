using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Queryz.Infrastructure;

public sealed class QueryzNavigation(IReportGroupService groups, IQueryzAuthorization access) : IShellNavigation
{
    public string? SearchUrlFormat => "/?q={0}";

    public async Task<IReadOnlyList<ForgeNavSection>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        var listed = await groups.ListAsync(cancellationToken);
        bool manage = await access.CanManageAsync();
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

        var sections = new List<ForgeNavSection>();
        if (manage)
        {
            sections.Add(new ForgeNavSection
            {
                Items =
                [
                    new ForgeNavItem { Title = "New report", Href = "/reports/new", Icon = "plus" },
                    new ForgeNavItem { Title = "Data sources", Href = "/data-sources", Icon = "database" },
                    new ForgeNavItem { Title = "Groups", Href = "/groups", Icon = "folder" },
                    new ForgeNavItem { Title = "Enumerations", Href = "/enumerations", Icon = "list" },
                ],
            });
        }

        sections.Add(new ForgeNavSection
        {
            Heading = "Groups",
            Items = items,
        });
        sections.Add(new ForgeNavSection
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
        });
        return sections;
    }
}
