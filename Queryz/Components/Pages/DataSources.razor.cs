using Microsoft.AspNetCore.Components;

namespace Queryz.Components.Pages;

public partial class DataSources
{
    [Inject]
    private IDataSourceService DataSourcesService { get; set; } = default!;

    [Inject]
    private IQueryzAuthorization Access { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    private bool loading = true;
    private bool forbidden;
    private bool loadingDatabases;
    private bool saving;
    private bool editing;
    private bool hasPassword;
    private string? error;
    private int? editingId;
    private SaveDataSourceRequest form = NewForm();
    private List<DataSourceDto> sources = [];
    private List<string> databases = [];

    private bool SqlIntegrated => form.Provider == ReportProvider.SqlServer && form.Connection.IntegratedSecurity;

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
        var listed = await DataSourcesService.ListAsync();
        loading = false;
        if (!listed.IsSuccess)
        {
            error = listed.GetDisplayMessage();
            return;
        }

        sources = [.. listed.Value];
    }

    private void Create()
    {
        editingId = null;
        hasPassword = false;
        form = NewForm();
        databases = [];
        error = null;
        editing = true;
    }

    private async Task EditAsync(int id)
    {
        var details = await DataSourcesService.GetDetailsAsync(id);
        if (!details.IsSuccess)
        {
            Toasts.ShowFailure(details);
            return;
        }

        editingId = id;
        hasPassword = details.Value.HasPassword;
        form = new SaveDataSourceRequest
        {
            Name = details.Value.Name,
            Provider = details.Value.Provider,
            Connection = details.Value.Connection,
        };
        databases = string.IsNullOrWhiteSpace(form.Connection.Database) ? [] : [form.Connection.Database];
        error = null;
        editing = true;
        await LoadDatabasesAsync();
    }

    private void Cancel()
    {
        editing = false;
        error = null;
        form = NewForm();
    }

    private void OnProviderChanged()
    {
        databases = [];
        form.Connection = new DataSourceConnectionRequest
        {
            Port = form.Provider switch
            {
                ReportProvider.PostgreSql => 5432,
                ReportProvider.MySql => 3306,
                _ => null,
            },
        };
    }

    private async Task LoadDatabasesAsync()
    {
        if (form.Provider == ReportProvider.Sqlite || string.IsNullOrWhiteSpace(form.Connection.Server))
        {
            return;
        }

        if (!SqlIntegrated && (string.IsNullOrWhiteSpace(form.Connection.UserId) || (string.IsNullOrEmpty(form.Connection.Password) && !hasPassword)))
        {
            return;
        }

        loadingDatabases = true;
        error = null;
        var listed = await DataSourcesService.ListDatabasesAsync(editingId, form);
        loadingDatabases = false;
        if (!listed.IsSuccess)
        {
            error = listed.GetDisplayMessage();
            return;
        }

        databases = [.. listed.Value];
        if (databases.Count > 0 && databases.All(name => !string.Equals(name, form.Connection.Database, StringComparison.OrdinalIgnoreCase)))
        {
            form.Connection.Database = databases[0];
        }
    }

    private async Task SaveAsync()
    {
        saving = true;
        error = null;
        var saved = await DataSourcesService.SaveAsync(editingId, form);
        saving = false;
        if (!saved.IsSuccess)
        {
            error = saved.GetDisplayMessage();
            Toasts.ShowFailure(saved);
            return;
        }

        editing = false;
        form = NewForm();
        Toasts.Show("Data source saved.", "success");
        await ReloadAsync();
    }

    private async Task TestAsync(int id)
    {
        var tested = await DataSourcesService.TestAsync(id);
        if (tested.IsSuccess)
        {
            Toasts.Show("Connected.", "success");
            return;
        }

        Toasts.ShowFailure(tested);
    }

    private async Task DeleteAsync(int id)
    {
        var deleted = await DataSourcesService.DeleteAsync(id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        await ReloadAsync();
    }

    private static SaveDataSourceRequest NewForm() => new()
    {
        Provider = ReportProvider.SqlServer,
    };

    private static string ProviderLabel(ReportProvider provider) => provider switch
    {
        ReportProvider.SqlServer => "SQL Server",
        ReportProvider.PostgreSql => "PostgreSQL",
        ReportProvider.MySql => "MySQL",
        ReportProvider.Sqlite => "SQLite",
        _ => provider.ToString(),
    };
}
