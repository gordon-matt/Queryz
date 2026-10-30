using Microsoft.AspNetCore.Components;
using QueryKrafter.Components;
using QueryKrafter.Models;
using QueryKrafter.Serialization;

namespace Queryz.Components.Pages;

public partial class RunReport
{
    [Inject]
    private IReportService ReportsService { get; set; } = default!;

    [Inject]
    private IQueryzAuthorization Access { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    [Parameter]
    public int Id { get; set; }

    private QueryBuilder? filters;
    private QueryBuilderConfig filterConfig = new();
    private QueryGroup rules = new();
    private ReportRunSetup? setup;
    private int loadedId;
    private bool loading = true;
    private bool running;
    private bool canManage;
    private string? error;
    private string? runError;
    private ReportResultDto? result;
    private string appliedRulesJson = string.Empty;
    private DownloadFileFormat fileFormat = DownloadFileFormat.Delimited;
    private DownloadFileDelimiter delimiter = DownloadFileDelimiter.Comma;
    private bool alwaysEnquote = true;
    private bool outputColumnNames = true;
    private bool downloadOpen;

    private string HeaderDescription => result is null
        ? "Set the filters, then run the report."
        : "The page shows the first rows. Download the file for the full report.";

    private void OpenDownload() => downloadOpen = true;

    private void CloseDownload() => downloadOpen = false;

    protected override async Task OnParametersSetAsync()
    {
        if (loadedId == Id && !loading)
        {
            return;
        }

        loadedId = Id;
        loading = true;
        running = false;
        result = null;
        runError = null;
        appliedRulesJson = string.Empty;
        downloadOpen = false;
        canManage = await Access.CanManageAsync();
        var prepared = await ReportsService.GetRunSetupAsync(Id);
        loading = false;

        if (!prepared.IsSuccess)
        {
            error = prepared.GetDisplayMessage();
            setup = null;
            return;
        }

        error = null;
        setup = prepared.Value;
        rules = prepared.Value.Rules;
        filterConfig = prepared.Value.Filters;
    }

    private async Task RunAsync()
    {
        if (filters is not null && !filters.Validate())
        {
            return;
        }

        running = true;
        runError = null;
        result = null;
        downloadOpen = false;
        string rulesJson = QueryJson.Serialize(rules);
        var ran = await ReportsService.RunAsync(Id, preview: true, rulesJson);
        running = false;

        if (!ran.IsSuccess)
        {
            runError = ran.GetDisplayMessage();
            return;
        }

        appliedRulesJson = rulesJson;
        result = ran.Value;
    }

    private async Task DeleteAsync()
    {
        string name = setup?.Name ?? result?.Name ?? "this report";
        bool confirmed = await Dialogs.ConfirmAsync(
            "Delete report",
            $"Delete \"{name}\"?",
            "Delete",
            isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        var deleted = await ReportsService.DeleteAsync(Id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        Navigation.NavigateTo("/");
    }
}
