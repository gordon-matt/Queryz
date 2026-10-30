using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace Queryz.Components.Pages;

public partial class Groups
{
    private static readonly int[] pageSizes = [10, 20, 50];

    [Inject]
    private IReportGroupService GroupsService { get; set; } = default!;

    [Inject]
    private IQueryzAuthorization Access { get; set; } = default!;

    [Inject]
    private RoleManager<ApplicationRole> RoleManager { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    private bool loading = true;
    private bool forbidden;
    private bool saving;
    private bool editing;
    private int? editingId;
    private string name = string.Empty;
    private string timeZoneId = string.Empty;
    private string? error;
    private int pageNumber = 1;
    private int pageSize = 10;
    private List<ReportGroupDto> groups = [];
    private List<ApplicationRole> roles = [];
    private readonly HashSet<string> selectedRoleIds = [];
    private readonly IReadOnlyList<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();

    private IEnumerable<ReportGroupDto> Page =>
        groups.Skip((pageNumber - 1) * pageSize).Take(pageSize);

    protected override async Task OnInitializedAsync()
    {
        if (!await Access.CanManageAsync())
        {
            forbidden = true;
            loading = false;
            return;
        }

        roles = RoleManager.Roles.OrderBy(role => role.Name).ToList();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        loading = true;
        var listed = await GroupsService.ListAsync();
        loading = false;
        if (!listed.IsSuccess)
        {
            error = listed.GetDisplayMessage();
            return;
        }

        groups = [.. listed.Value];
        int pageCount = Math.Max(1, (int)Math.Ceiling(groups.Count / (double)pageSize));
        pageNumber = Math.Min(pageNumber, pageCount);
    }

    private void Create()
    {
        editingId = null;
        name = string.Empty;
        timeZoneId = string.Empty;
        selectedRoleIds.Clear();
        error = null;
        editing = true;
    }

    private void Edit(ReportGroupDto group)
    {
        editingId = group.Id;
        name = group.Name;
        timeZoneId = group.TimeZoneId;
        selectedRoleIds.Clear();
        foreach (string roleId in group.RoleIds)
        {
            selectedRoleIds.Add(roleId);
        }

        error = null;
        editing = true;
    }

    private void Cancel()
    {
        editing = false;
        error = null;
        name = string.Empty;
        timeZoneId = string.Empty;
        selectedRoleIds.Clear();
    }

    private void ToggleRole(string roleId)
    {
        if (!selectedRoleIds.Remove(roleId))
        {
            selectedRoleIds.Add(roleId);
        }
    }

    private string TimeZoneLabel(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        return timeZones.FirstOrDefault(zone => zone.Id == id)?.DisplayName ?? id;
    }

    private async Task SaveAsync()
    {
        saving = true;
        var saved = await GroupsService.SaveAsync(editingId, new SaveReportGroupRequest
        {
            Name = name,
            TimeZoneId = timeZoneId,
            RoleIds = [.. selectedRoleIds],
        });
        saving = false;
        if (!saved.IsSuccess)
        {
            error = saved.GetDisplayMessage();
            Toasts.ShowFailure(saved);
            return;
        }

        editing = false;
        name = string.Empty;
        timeZoneId = string.Empty;
        selectedRoleIds.Clear();
        error = null;
        Toasts.Show("Group saved.", "success");
        await ReloadAsync();
    }

    private async Task DeleteAsync(ReportGroupDto group)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            "Delete group",
            $"Delete \"{group.Name}\"?",
            "Delete",
            isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        var deleted = await GroupsService.DeleteAsync(group.Id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        await ReloadAsync();
    }

    private void OnPageChanged(int page) => pageNumber = page;

    private void OnPageSizeChanged(int size)
    {
        pageSize = size;
        pageNumber = 1;
    }
}
