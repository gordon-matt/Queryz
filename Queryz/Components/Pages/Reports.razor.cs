using Microsoft.AspNetCore.Components;

namespace Queryz.Components.Pages;

public partial class Reports
{
    [Inject]
    private IReportService ReportsService { get; set; } = default!;

    [Inject]
    private IQueryzAuthorization Access { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "group")]
    public string? Group { get; set; }

    [SupplyParameterFromQuery(Name = "q")]
    public string? Query { get; set; }

    private bool loading = true;
    private bool canManage;
    private string? loadError;
    private List<ReportSummaryDto> visible = [];

    protected override async Task OnParametersSetAsync()
    {
        loading = true;
        canManage = await Access.CanManageAsync();
        var listed = await ReportsService.ListAsync();
        if (!listed.IsSuccess)
        {
            loadError = listed.GetDisplayMessage();
            visible = [];
            loading = false;
            return;
        }

        loadError = null;
        IEnumerable<ReportSummaryDto> rows = listed.Value;

        if (!string.IsNullOrWhiteSpace(Group))
        {
            rows = rows.Where(report => string.Equals(report.GroupName, Group, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(Query))
        {
            rows = rows.Where(report => report.Name.Contains(Query, StringComparison.OrdinalIgnoreCase));
        }

        visible = rows.ToList();
        loading = false;
    }
}
