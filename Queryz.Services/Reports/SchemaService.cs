using System.Data;

namespace Queryz.Services.Reports;

public interface ISchemaService
{
    Task<Result<IReadOnlyList<SchemaTableDto>>> GetTablesAsync(int dataSourceId, CancellationToken cancellationToken = default);
}

public sealed class SchemaService(
    ILogger<SchemaService> logger,
    IQueryzAuthorization access,
    IRepository<DataSource> dataSources,
    IRepository<Report> reports,
    IRepository<ReportGroupRole> groupRoles,
    IRepository<ReportUserBlacklistEntry> blacklist) : ISchemaService
{
    public async Task<Result<IReadOnlyList<SchemaTableDto>>> GetTablesAsync(
        int dataSourceId,
        CancellationToken cancellationToken = default)
    {
        if (!await access.CanManageAsync() && !await CanReadForRunAsync(dataSourceId, cancellationToken))
        {
            return Result<IReadOnlyList<SchemaTableDto>>.Forbidden();
        }

        var source = await dataSources.FindOneAsync(dataSourceId);
        if (source is null)
        {
            return Result<IReadOnlyList<SchemaTableDto>>.NotFound();
        }

        try
        {
            using var connection = ReportSql.Open(source.Provider, source.ConnectionString);
            var tables = new List<SchemaTableDto>();

            foreach (DataRow table in connection.GetSchema("Tables").Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string? tableType = Column(table, "TABLE_TYPE");
                if (tableType is not null
                    && !tableType.Equals("TABLE", StringComparison.OrdinalIgnoreCase)
                    && !tableType.Equals("BASE TABLE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string? schema = Column(table, "TABLE_SCHEMA");
                string? name = Column(table, "TABLE_NAME");
                if (string.IsNullOrWhiteSpace(name) || name.StartsWith("sqlite_", StringComparison.Ordinal))
                {
                    continue;
                }

                string qualified = string.IsNullOrWhiteSpace(schema) || schema.Equals("main", StringComparison.OrdinalIgnoreCase)
                    ? name
                    : schema + "." + name;

                var columns = connection.GetSchema("Columns", SchemaRestrictions(schema, name))
                    .Rows
                    .Cast<DataRow>()
                    .Select(row => new SchemaColumnDto(
                        Column(row, "COLUMN_NAME") ?? string.Empty,
                        Column(row, "DATA_TYPE") ?? "string"))
                    .Where(column => column.Name.Length > 0)
                    .ToList();

                tables.Add(new SchemaTableDto(qualified, columns));
            }

            return Result.Success<IReadOnlyList<SchemaTableDto>>(tables.OrderBy(table => table.Name, StringComparer.OrdinalIgnoreCase).ToList());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Schema for data source {DataSourceId} could not be read", dataSourceId);
            return Result<IReadOnlyList<SchemaTableDto>>.Error(ex.Message);
        }
    }

    private async Task<bool> CanReadForRunAsync(int dataSourceId, CancellationToken cancellationToken)
    {
        if (!await access.CanUseAsync())
        {
            return false;
        }

        var owned = await reports.FindAsync(new SearchOptions<Report>
        {
            Query = report => report.DataSourceId == dataSourceId && report.Enabled,
            CancellationToken = cancellationToken,
        });
        if (owned.Count == 0)
        {
            return false;
        }

        var groupIds = owned.Select(report => report.GroupId).Distinct().ToList();
        var reportIds = owned.Select(report => report.Id).ToList();
        var roles = await groupRoles.FindAsync(new SearchOptions<ReportGroupRole>
        {
            Query = role => groupIds.Contains(role.ReportGroupId),
            CancellationToken = cancellationToken,
        });
        var denied = await blacklist.FindAsync(new SearchOptions<ReportUserBlacklistEntry>
        {
            Query = entry => reportIds.Contains(entry.ReportId),
            CancellationToken = cancellationToken,
        });
        var rolesByGroup = roles.GroupBy(role => role.ReportGroupId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.Select(role => role.RoleId).ToList());
        var deniedByReport = denied.GroupBy(entry => entry.ReportId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.Select(entry => entry.UserId).ToList());

        foreach (var report in owned)
        {
            if (await access.CanRunAsync(
                rolesByGroup.GetValueOrDefault(report.GroupId) ?? [],
                deniedByReport.GetValueOrDefault(report.Id) ?? []))
            {
                return true;
            }
        }

        return false;
    }

    private static string?[] SchemaRestrictions(string? schema, string table) =>
        [null, string.IsNullOrWhiteSpace(schema) ? null : schema, table, null];

    private static string? Column(DataRow row, string name) =>
        row.Table.Columns.Contains(name) && row[name] != DBNull.Value ? Convert.ToString(row[name]) : null;
}
