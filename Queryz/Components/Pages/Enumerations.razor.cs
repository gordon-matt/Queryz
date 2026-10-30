using Microsoft.AspNetCore.Components;

namespace Queryz.Components.Pages;

public partial class Enumerations
{
    private static readonly int[] pageSizes = [10, 20, 50];

    [Inject]
    private IEnumerationService EnumerationsService { get; set; } = default!;

    [Inject]
    private IQueryzAuthorization Access { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    private bool loading = true;
    private bool forbidden;
    private bool saving;
    private bool editing;
    private bool isBitFlags;
    private int? editingId;
    private string name = string.Empty;
    private string? error;
    private int pageNumber = 1;
    private int pageSize = 10;
    private List<EnumerationDto> enumerations = [];
    private List<EnumerationValueInput> values = [];

    private IEnumerable<EnumerationDto> Page =>
        enumerations.Skip((pageNumber - 1) * pageSize).Take(pageSize);

    protected override async Task OnInitializedAsync()
    {
        if (!await Access.CanManageAsync())
        {
            forbidden = true;
            loading = false;
            return;
        }

        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        loading = true;
        var listed = await EnumerationsService.ListAsync();
        loading = false;
        if (!listed.IsSuccess)
        {
            error = listed.GetDisplayMessage();
            return;
        }

        enumerations = [.. listed.Value];
        int pageCount = Math.Max(1, (int)Math.Ceiling(enumerations.Count / (double)pageSize));
        pageNumber = Math.Min(pageNumber, pageCount);
    }

    private void Create()
    {
        editingId = null;
        name = string.Empty;
        isBitFlags = false;
        values = [];
        error = null;
        editing = true;
    }

    private void Edit(EnumerationDto item)
    {
        editingId = item.Id;
        name = item.Name;
        isBitFlags = item.IsBitFlags;
        values = item.Values.Select(value => new EnumerationValueInput { Id = value.Id, Name = value.Name }).ToList();
        error = null;
        editing = true;
    }

    private void Cancel()
    {
        editing = false;
        error = null;
    }

    private void AddValue()
    {
        int next = values.Count == 0
            ? (isBitFlags ? 1 : 0)
            : isBitFlags ? NextFlag(values.Max(value => value.Id)) : values.Max(value => value.Id) + 1;
        values.Add(new EnumerationValueInput { Id = next });
    }

    private async Task RemoveValueAsync(EnumerationValueInput value)
    {
        bool confirmed = await Dialogs.ConfirmAsync("Remove value", "Remove this value?", "Remove", isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        values.Remove(value);
    }

    private static int NextFlag(int current)
    {
        if (current < 1)
        {
            return 1;
        }

        int flag = 1;
        while (flag <= current && flag < int.MaxValue / 2)
        {
            flag <<= 1;
        }

        return flag;
    }

    private async Task SaveAsync()
    {
        saving = true;
        var saved = await EnumerationsService.SaveAsync(editingId, new SaveEnumerationRequest
        {
            Name = name,
            IsBitFlags = isBitFlags,
            Values = values,
        });
        saving = false;
        if (!saved.IsSuccess)
        {
            error = saved.GetDisplayMessage();
            Toasts.ShowFailure(saved);
            return;
        }

        editing = false;
        error = null;
        Toasts.Show("Enumeration saved.", "success");
        await ReloadAsync();
    }

    private async Task DeleteAsync(EnumerationDto item)
    {
        bool confirmed = await Dialogs.ConfirmAsync(
            "Delete enumeration",
            $"Delete \"{item.Name}\"?",
            "Delete",
            isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        var deleted = await EnumerationsService.DeleteAsync(item.Id);
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
