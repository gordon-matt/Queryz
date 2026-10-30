using Microsoft.AspNetCore.Components;

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

    [Parameter]
    public int Id { get; set; }

    private bool loading = true;
    private bool canManage;
    private string? error;
    private ReportResultDto? result;
    private DownloadFileFormat fileFormat = DownloadFileFormat.Delimited;
    private DownloadFileDelimiter delimiter = DownloadFileDelimiter.Comma;
    private bool alwaysEnquote = true;
    private bool outputColumnNames = true;

    protected override async Task OnParametersSetAsync()
    {
        loading = true;
        canManage = await Access.CanManageAsync();
        var ran = await ReportsService.RunAsync(Id, preview: true);
        loading = false;

        if (!ran.IsSuccess)
        {
            error = ran.GetDisplayMessage();
            result = null;
            return;
        }

        error = null;
        result = ran.Value;
    }

    private async Task DeleteAsync()
    {
        var deleted = await ReportsService.DeleteAsync(Id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        Navigation.NavigateTo("/");
    }
}
