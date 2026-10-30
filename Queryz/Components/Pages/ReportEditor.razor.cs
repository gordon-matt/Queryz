using Heroicsoft.NetForge.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using QueryKrafter.Models;
using QueryKrafter.Serialization;

namespace Queryz.Components.Pages;

public partial class ReportEditor
{
    [Inject]
    private IReportService ReportsService { get; set; } = default!;

    [Inject]
    private IQueryzAuthorization Access { get; set; } = default!;

    [Inject]
    private IDataSourceService DataSources { get; set; } = default!;

    [Inject]
    private IReportGroupService Groups { get; set; } = default!;

    [Inject]
    private IEnumerationService EnumerationsService { get; set; } = default!;

    [Inject]
    private UserManager<ApplicationUser> Users { get; set; } = default!;

    [Inject]
    private RoleManager<ApplicationRole> Roles { get; set; } = default!;

    [Inject]
    private ISchemaService Schema { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    [Inject]
    private DialogService Dialogs { get; set; } = default!;

    [Parameter]
    public int? Id { get; set; }

    private bool loading = true;
    private bool forbidden;
    private bool missing;
    private bool saving;
    private string? error;
    private SaveReportRequest form = new();
    private List<DataSourceDto> sources = [];
    private List<ReportGroupDto> groups = [];
    private List<EnumerationDto> enumerations = [];
    private List<BlacklistUser> candidates = [];
    private List<SchemaTableDto> tables = [];
    private readonly List<string> selectedTables = [];
    private int schemaSourceId;
    private bool columnsCustomized;
    private QueryGroup rules = new();
    private QueryBuilderConfig filterConfig = new();

    private IReadOnlyList<SchemaTableDto> ChosenTables =>
        selectedTables
            .Select(name => tables.FirstOrDefault(table => string.Equals(table.Name, name, StringComparison.OrdinalIgnoreCase)))
            .Where(table => table is not null)
            .Cast<SchemaTableDto>()
            .ToList();

    private IReadOnlyList<SchemaColumnDto> CurrentColumns => ReportService.ColumnsFor(ChosenTables);

    private IEnumerable<string> SortableColumns => form.Columns
        .Where(column => !column.IsHidden)
        .Select(column => column.IsLiteral ? column.Alias : column.Name)
        .Where(name => !string.IsNullOrWhiteSpace(name));

    protected override async Task OnInitializedAsync()
    {
        if (!await Access.CanManageAsync())
        {
            forbidden = true;
            loading = false;
            return;
        }

        var sourceResult = await DataSources.ListAsync();
        var groupResult = await Groups.ListAsync();
        sources = sourceResult.IsSuccess ? [.. sourceResult.Value] : [];
        groups = groupResult.IsSuccess ? [.. groupResult.Value] : [];
        var enumerationResult = await EnumerationsService.ListAsync();
        enumerations = enumerationResult.IsSuccess ? [.. enumerationResult.Value] : [];

        if (Id is int id)
        {
            var found = await ReportsService.GetAsync(id);
            if (!found.IsSuccess)
            {
                missing = true;
                loading = false;
                return;
            }

            form = new SaveReportRequest
            {
                Name = found.Value.Name,
                GroupId = found.Value.GroupId,
                DataSourceId = found.Value.DataSourceId,
                TableName = found.Value.TableName,
                IsDistinct = found.Value.IsDistinct,
                RowLimit = found.Value.RowLimit,
                Enabled = found.Value.Enabled,
                EmailEnabled = found.Value.EmailEnabled,
                EnumerationHandling = found.Value.EnumerationHandling,
                RulesJson = found.Value.RulesJson,
                DeniedUserIds = [.. found.Value.DeniedUserIds],
                Columns = found.Value.Columns.Select(column => new ReportColumnInput
                {
                    Name = column.Name,
                    Alias = column.Alias,
                    IsLiteral = column.IsLiteral,
                    IsHidden = column.IsHidden,
                    Format = column.Format,
                    EnumerationId = column.EnumerationId ?? 0,
                    TransformFunction = column.TransformFunction,
                }).ToList(),
                Tables = found.Value.Tables.Select(table => new ReportTableInput
                {
                    Name = table.Name,
                    ParentTable = table.ParentTable,
                    PrimaryKey = table.PrimaryKey,
                    ForeignKey = table.ForeignKey,
                    JoinType = table.JoinType,
                }).ToList(),
                Sorts = found.Value.Sorts.Select(sort => new ReportSortInput
                {
                    ColumnName = sort.ColumnName,
                    Direction = sort.Direction,
                }).ToList(),
            };

            selectedTables.AddRange(form.Tables.Select(table => table.Name));
            schemaSourceId = found.Value.DataSourceId;
            columnsCustomized = form.Columns.Count > 0;

            var parsed = QueryJson.TryParse(found.Value.RulesJson);
            rules = parsed.IsSuccess ? parsed.Value : new QueryGroup();
        }
        else
        {
            form.GroupId = groups.FirstOrDefault()?.Id ?? 0;
            form.DataSourceId = sources.FirstOrDefault()?.Id ?? 0;
            form.Enabled = true;
        }

        await LoadCandidatesAsync();
        loading = false;
    }

    private async Task<bool> LeaveSettingsAsync()
    {
        error = null;
        if (string.IsNullOrWhiteSpace(form.Name))
        {
            error = "The name is required.";
            return false;
        }

        if (form.GroupId == 0)
        {
            error = "Choose a group.";
            return false;
        }

        if (form.DataSourceId == 0)
        {
            error = "Choose a data source.";
            return false;
        }

        var schema = await Schema.GetTablesAsync(form.DataSourceId);
        if (!schema.IsSuccess)
        {
            error = schema.GetDisplayMessage();
            return false;
        }

        tables = [.. schema.Value];
        bool sourceChanged = form.DataSourceId != schemaSourceId;
        schemaSourceId = form.DataSourceId;
        if (sourceChanged)
        {
            selectedTables.Clear();
            form.Columns = [];
            form.Tables = [];
            form.Sorts = [];
            columnsCustomized = false;
        }
        else
        {
            selectedTables.RemoveAll(name => tables.All(table => !string.Equals(table.Name, name, StringComparison.OrdinalIgnoreCase)));
            if (selectedTables.Count == 0)
            {
                form.Columns = [];
                form.Tables = [];
                columnsCustomized = false;
            }
        }

        return await RefreshCandidatesAsync();
    }

    private Task<bool> LeaveTables()
    {
        error = null;
        if (selectedTables.Count == 0)
        {
            error = "Select a table.";
            return Task.FromResult(false);
        }

        SyncTableRows();
        var offered = CurrentColumns.Select(column => column.Name).ToList();
        if (!columnsCustomized)
        {
            form.Columns = offered.Select(name => new ReportColumnInput
            {
                Name = name,
                Alias = ReportSql.AliasFor(name),
            }).ToList();
        }
        else
        {
            form.Columns = AlignColumns(form.Columns, offered);
            if (form.Columns.Count == 0)
            {
                form.Columns = offered.Select(name => new ReportColumnInput
                {
                    Name = name,
                    Alias = ReportSql.AliasFor(name),
                }).ToList();
            }
        }

        SyncColumns();
        return Task.FromResult(true);
    }

    private Task<bool> LeaveColumns()
    {
        error = null;
        if (form.Columns.All(column => column.IsHidden || string.IsNullOrWhiteSpace(column.Name)))
        {
            error = "Add at least one column.";
            return Task.FromResult(false);
        }

        if (form.Columns.Where(column => !column.IsLiteral).GroupBy(column => column.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            error = "A column is selected more than once.";
            return Task.FromResult(false);
        }

        if (form.Columns.Any(column => column.IsLiteral && string.IsNullOrWhiteSpace(column.Alias)))
        {
            error = "A literal column needs an alias.";
            return Task.FromResult(false);
        }

        SyncColumns();
        return Task.FromResult(true);
    }

    private Task<bool> LeaveSorting()
    {
        error = null;
        if (form.Sorts.Any(sort => string.IsNullOrWhiteSpace(sort.ColumnName)))
        {
            error = "Choose a column for each sort.";
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private async Task<bool> LeaveRelationships()
    {
        error = null;
        SyncTableRows();
        error = ReportSql.ValidateJoins(form.Tables);
        if (error is not null)
        {
            return false;
        }

        return await RefreshCandidatesAsync();
    }

    private async Task<bool> RefreshCandidatesAsync()
    {
        await LoadCandidatesAsync();
        return true;
    }

    private async Task LoadCandidatesAsync()
    {
        candidates = [];
        var group = groups.FirstOrDefault(item => item.Id == form.GroupId);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (group is not null && group.RoleIds.Count > 0)
        {
            var roleIds = group.RoleIds.ToList();
            foreach (var role in Roles.Roles.Where(role => roleIds.Contains(role.Id) && role.Name != null).ToList())
            {
                foreach (var user in await Users.GetUsersInRoleAsync(role.Name!))
                {
                    if (seen.Add(user.Id))
                    {
                        candidates.Add(new BlacklistUser(user.Id, Display(user)));
                    }
                }
            }
        }

        foreach (string deniedId in form.DeniedUserIds.Where(id => !seen.Contains(id)))
        {
            var user = await Users.FindByIdAsync(deniedId);
            candidates.Add(new BlacklistUser(deniedId, user is null ? deniedId : Display(user)));
        }

        candidates.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
    }

    private static string Display(ApplicationUser user) =>
        string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName ?? user.Email ?? user.Id : user.DisplayName;

    private void ToggleDenied(string userId)
    {
        if (!form.DeniedUserIds.Remove(userId))
        {
            form.DeniedUserIds.Add(userId);
        }
    }

    private sealed record BlacklistUser(string Id, string Name);

    private void SyncTableRows()
    {
        var next = new List<ReportTableInput>();
        foreach (string name in selectedTables)
        {
            var existing = form.Tables.FirstOrDefault(table => string.Equals(table.Name, name, StringComparison.OrdinalIgnoreCase));
            next.Add(existing ?? new ReportTableInput { Name = name });
            next[^1].Name = name;
        }

        if (next.Count > 0 && next.All(table => !string.IsNullOrWhiteSpace(table.ParentTable)))
        {
            next[0].ParentTable = string.Empty;
            next[0].PrimaryKey = string.Empty;
            next[0].ForeignKey = string.Empty;
        }

        form.Tables = next;
        form.TableName = next.First(table => string.IsNullOrWhiteSpace(table.ParentTable)).Name;
    }

    private void ToggleTable(string name)
    {
        int index = selectedTables.FindIndex(table => string.Equals(table, name, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            selectedTables.RemoveAt(index);
            return;
        }

        selectedTables.Add(name);
    }

    private bool IsTableSelected(string name) =>
        selectedTables.Any(table => string.Equals(table, name, StringComparison.OrdinalIgnoreCase));

    private IEnumerable<string> ParentChoices(string tableName) =>
        selectedTables.Where(name => !string.Equals(name, tableName, StringComparison.OrdinalIgnoreCase));

    private IEnumerable<string> ColumnsOf(string tableName) =>
        tables.FirstOrDefault(table => string.Equals(table.Name, tableName, StringComparison.OrdinalIgnoreCase))
            ?.Columns.Select(column => column.Name)
        ?? [];

    private static List<ReportColumnInput> AlignColumns(IReadOnlyList<ReportColumnInput> current, IReadOnlyList<string> offered)
    {
        var aligned = new List<ReportColumnInput>();
        foreach (var column in current)
        {
            if (column.IsLiteral)
            {
                aligned.Add(column);
                continue;
            }

            string? exact = offered.FirstOrDefault(item => string.Equals(item, column.Name, StringComparison.OrdinalIgnoreCase));
            if (exact is not null)
            {
                column.Name = exact;
                aligned.Add(column);
                continue;
            }

            var bare = offered.Where(item => item.EndsWith("." + column.Name, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
            if (bare.Count == 1)
            {
                column.Name = bare[0];
                if (string.IsNullOrWhiteSpace(column.Alias))
                {
                    column.Alias = ReportSql.AliasFor(bare[0]);
                }

                aligned.Add(column);
            }
        }

        return aligned;
    }

    private void SyncColumns() => filterConfig = ReportsService.Filters(CurrentColumns);

    private void AddColumn()
    {
        string? next = CurrentColumns.Select(column => column.Name)
            .FirstOrDefault(name => !form.Columns.Any(column => !column.IsLiteral && string.Equals(column.Name, name, StringComparison.OrdinalIgnoreCase)));
        if (next is not null)
        {
            form.Columns.Add(new ReportColumnInput { Name = next, Alias = ReportSql.AliasFor(next) });
            columnsCustomized = true;
        }
    }

    private void AddLiteral()
    {
        form.Columns.Add(new ReportColumnInput { IsLiteral = true });
        columnsCustomized = true;
    }

    private void ChangeColumn(int index, ChangeEventArgs args)
    {
        string? name = args.Value?.ToString();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        form.Columns[index].Name = name;
        form.Columns[index].Alias = ReportSql.AliasFor(name);
        columnsCustomized = true;
    }

    private int? draggedColumn;

    private int? dragOverColumn;

    private void BeginColumnDrag(int index) => draggedColumn = index;

    private void DragColumnOver(int index)
    {
        if (draggedColumn is not null)
        {
            dragOverColumn = index;
        }
    }

    private void DropColumn(int index)
    {
        if (draggedColumn is int from && from != index && from >= 0 && from < form.Columns.Count)
        {
            var column = form.Columns[from];
            form.Columns.RemoveAt(from);
            int target = index;
            if (target > form.Columns.Count)
            {
                target = form.Columns.Count;
            }

            form.Columns.Insert(target, column);
            columnsCustomized = true;
        }

        EndColumnDrag();
    }

    private void EndColumnDrag()
    {
        draggedColumn = null;
        dragOverColumn = null;
    }

    private async Task RemoveColumnAsync(int index)
    {
        bool confirmed = await Dialogs.ConfirmAsync("Remove column", "Remove this column?", "Remove", isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        form.Columns.RemoveAt(index);
        columnsCustomized = true;
    }

    private async Task ClearColumnsAsync()
    {
        bool confirmed = await Dialogs.ConfirmAsync("Remove all columns", "Remove every selected column?", "Remove", isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        form.Columns.Clear();
        columnsCustomized = true;
    }

    private void AddSort() => form.Sorts.Add(new ReportSortInput());

    private async Task RemoveSortAsync(int index)
    {
        bool confirmed = await Dialogs.ConfirmAsync("Remove sort", "Remove this sort?", "Remove", isDestructive: true);
        if (!confirmed)
        {
            return;
        }

        form.Sorts.RemoveAt(index);
    }

    private void MoveSort(int index, int delta)
    {
        int target = index + delta;
        if (target < 0 || target >= form.Sorts.Count)
        {
            return;
        }

        (form.Sorts[index], form.Sorts[target]) = (form.Sorts[target], form.Sorts[index]);
    }

    private string ColumnType(string name) =>
        CurrentColumns.FirstOrDefault(column => string.Equals(column.Name, name, StringComparison.OrdinalIgnoreCase))?.DataType ?? string.Empty;

    private async Task SaveAsync()
    {
        saving = true;
        error = null;
        SyncTableRows();
        error = ReportSql.ValidateJoins(form.Tables);
        if (error is not null)
        {
            saving = false;
            return;
        }

        form.RulesJson = QueryJson.Serialize(rules);
        var saved = await ReportsService.SaveAsync(Id, form);
        saving = false;

        if (!saved.IsSuccess)
        {
            error = saved.GetDisplayMessage();
            Toasts.ShowFailure(saved);
            return;
        }

        Toasts.Show(Id is null ? "Report created." : "Report saved.", "success");
        Navigation.NavigateTo($"/reports/{saved.Value.Id}");
    }
}
