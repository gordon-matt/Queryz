using Heroicsoft.NetForge.Components.Navigation;

namespace Queryz.Infrastructure;

/// <summary>
/// Puts one Queryz link on the host menu. The pages are the shell sidebar, not children of this link,
/// so another app shows "Queryz" the way it shows "Logs".
/// </summary>
public sealed class QueryzMenuContributor : INetForgeMenuContributor
{
    public string Name => "Queryz";

    public string DefaultMenu => "Reports";

    public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure) => group.AddCustom(item =>
    {
        item.Title = "Queryz";
        item.Icon = "library";
        item.Url = "/";
        item.MatchAll = true;
        configure?.Invoke(item);
    });
}

/// <summary>Places the Queryz link. The host can retitle it; the address stays the reports page.</summary>
public static class QueryzMenuExtensions
{
    extension(INetForgeMenuGroupBuilder menu)
    {
        public INetForgeMenuGroupBuilder AddQueryz(Action<NetForgeMenuItemOptions>? configure = null) =>
            menu.AddContributor("Queryz", configure);
    }
}