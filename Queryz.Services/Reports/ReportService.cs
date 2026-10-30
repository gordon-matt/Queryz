using System.Data;
using QueryKrafter.Models;
using QueryKrafter.Sql;

namespace Queryz.Services.Reports;

public interface IReportService
{
    Task<Result<IReadOnlyList<ReportSummaryDto>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<ReportDefinitionDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ReportDefinitionDto>> SaveAsync(int? id, SaveReportRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ReportRunSetup>> GetRunSetupAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ReportResultDto>> RunAsync(int id, bool preview, string? rulesJson = null, CancellationToken cancellationToken = default);

    QueryBuilderConfig Filters(IReadOnlyList<SchemaColumnDto> columns);
}

/// <summary>The filter step shown before a report runs.</summary>
public sealed record ReportRunSetup(string Name, QueryBuilderConfig Filters, QueryGroup Rules);

public sealed class ReportService(
    ILogger<ReportService> logger,
    IUserContextService userContext,
    IQueryzAuthorization access,
    TimeProvider time,
    IRepository<Report> reports,
    IRepository<ReportColumn> columns,
    IRepository<ReportTable> tables,
    IRepository<ReportSorting> sortings,
    IRepository<ReportUserBlacklistEntry> blacklist,
    IRepository<ReportGroupRole> groupRoles,
    IRepository<Enumeration> enumerations,
    IRepository<DataSource> dataSources,
    IRepository<ReportGroup> groups,
    ISchemaService schema) : IReportService
{
    public async Task<Result<IReadOnlyList<ReportSummaryDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (!await access.CanUseAsync())
        {
            return Result<IReadOnlyList<ReportSummaryDto>>.Forbidden();
        }

        var rows = await reports.FindAsync(new SearchOptions<Report>
        {
            OrderBy = query => query.OrderBy(report => report.Name),
            CancellationToken = cancellationToken,
        });

        bool manage = await access.CanManageAsync();
        var rolesByGroup = await RolesByGroupAsync(cancellationToken);
        var deniedByReport = await DeniedByReportAsync(cancellationToken);
        IEnumerable<Report> listed = rows;
        if (!manage)
        {
            var allowed = new List<Report>();
            foreach (var report in rows)
            {
                if (!report.Enabled)
                {
                    continue;
                }

                if (await access.CanRunAsync(rolesByGroup.GetValueOrDefault(report.GroupId) ?? [], deniedByReport.GetValueOrDefault(report.Id) ?? []))
                {
                    allowed.Add(report);
                }
            }

            listed = allowed;
        }

        var sourceNames = (await dataSources.FindAsync(new SearchOptions<DataSource> { CancellationToken = cancellationToken }))
            .ToDictionary(source => source.Id, source => source.Name);
        var groupNames = (await groups.FindAsync(new SearchOptions<ReportGroup> { CancellationToken = cancellationToken }))
            .ToDictionary(group => group.Id, group => group.Name);

        var summaries = listed.Select(report => new ReportSummaryDto(
            report.Id,
            report.Name,
            groupNames.GetValueOrDefault(report.GroupId, string.Empty),
            sourceNames.GetValueOrDefault(report.DataSourceId, string.Empty),
            report.Enabled)).ToList();

        return Result.Success<IReadOnlyList<ReportSummaryDto>>(summaries);
    }

    public async Task<Result<ReportDefinitionDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var report = await reports.FindOneAsync(id);
        if (report is null)
        {
            return Result<ReportDefinitionDto>.NotFound();
        }

        if (!await MayViewAsync(report, cancellationToken))
        {
            return Result<ReportDefinitionDto>.Forbidden();
        }

        var selected = await columns.FindAsync(new SearchOptions<ReportColumn>
        {
            Query = column => column.ReportId == id,
            OrderBy = query => query.OrderBy(column => column.Ordinal),
            CancellationToken = cancellationToken,
        });

        var tableRows = await tables.FindAsync(new SearchOptions<ReportTable>
        {
            Query = table => table.ReportId == id,
            OrderBy = query => query.OrderBy(table => table.Ordinal),
            CancellationToken = cancellationToken,
        });

        var sortRows = await sortings.FindAsync(new SearchOptions<ReportSorting>
        {
            Query = sort => sort.ReportId == id,
            OrderBy = query => query.OrderBy(sort => sort.Ordinal),
            CancellationToken = cancellationToken,
        });

        var denied = await blacklist.FindAsync(new SearchOptions<ReportUserBlacklistEntry>
        {
            Query = entry => entry.ReportId == id,
            CancellationToken = cancellationToken,
        });

        return Result.Success(ToDefinition(report, tableRows, selected, sortRows, denied.Select(entry => entry.UserId).ToList()));
    }

    public async Task<Result<ReportDefinitionDto>> SaveAsync(
        int? id,
        SaveReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await access.CanManageAsync())
        {
            return Result<ReportDefinitionDto>.Forbidden();
        }

        string? joinError = ReportSql.ValidateJoins(request.Tables);
        if (joinError is not null)
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError(joinError));
        }

        var tableNames = request.Tables.Select(table => table.Name.Trim()).ToList();
        if (string.IsNullOrWhiteSpace(request.Name) || tableNames.Any(string.IsNullOrWhiteSpace))
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("The name and at least one table are required."));
        }

        var visibleColumns = request.Columns.Where(column => !column.IsHidden && !string.IsNullOrWhiteSpace(column.Name)).ToList();
        if (visibleColumns.Count == 0)
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("Choose at least one column that is not hidden."));
        }

        if (request.Columns.Where(column => !column.IsLiteral).GroupBy(column => column.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("A column is selected more than once."));
        }

        if (request.Columns.Any(column => column.IsLiteral && string.IsNullOrWhiteSpace(column.Alias)))
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("A literal column needs an alias."));
        }

        if (request.Columns.Count == 0)
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("Choose at least one column."));
        }

        if (await dataSources.FindOneAsync(request.DataSourceId) is null)
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("Choose a data source."));
        }

        if (await groups.FindOneAsync(request.GroupId) is null)
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("Choose a group."));
        }

        string masterTable = request.Tables.First(table => string.IsNullOrWhiteSpace(table.ParentTable)).Name.Trim();
        var report = id is null ? new Report { Name = request.Name.Trim(), TableName = masterTable } : await reports.FindOneAsync(id.Value);
        if (report is null)
        {
            return Result<ReportDefinitionDto>.NotFound();
        }

        report.Name = request.Name.Trim();
        report.GroupId = request.GroupId;
        report.DataSourceId = request.DataSourceId;
        report.TableName = masterTable;
        report.IsDistinct = request.IsDistinct;
        report.RowLimit = request.RowLimit is > 0 ? Math.Min(request.RowLimit.Value, Constants.MaxReportRows) : null;
        if (request.Columns.Any(column => !string.IsNullOrWhiteSpace(column.TransformFunction) && !ReportTransforms.All.Contains(column.TransformFunction)))
        {
            return Result<ReportDefinitionDto>.Invalid(new ValidationError("Choose a known transform."));
        }

        var enumerationIds = request.Columns.Where(column => column.EnumerationId > 0).Select(column => column.EnumerationId).Distinct().ToList();
        if (enumerationIds.Count > 0)
        {
            int foundEnumerations = await enumerations.CountAsync(item => enumerationIds.Contains(item.Id));
            if (foundEnumerations != enumerationIds.Count)
            {
                return Result<ReportDefinitionDto>.Invalid(new ValidationError("A column uses an enumeration that does not exist."));
            }
        }

        report.Enabled = request.Enabled;
        report.EmailEnabled = request.EmailEnabled;
        report.EnumerationHandling = request.EnumerationHandling;
        report.RulesJson = request.RulesJson ?? string.Empty;
        report.UpdatedAt = time.GetUtcNow().UtcDateTime;

        if (id is null)
        {
            await reports.InsertAsync(report);
            logger.LogInformation("User {UserId} created report {ReportId}", userContext.GetCurrentUserId(), report.Id);
        }
        else
        {
            await reports.UpdateAsync(report);
            await columns.DeleteAsync(column => column.ReportId == report.Id);
            await tables.DeleteAsync(table => table.ReportId == report.Id);
            await sortings.DeleteAsync(sort => sort.ReportId == report.Id);
            await blacklist.DeleteAsync(entry => entry.ReportId == report.Id);
            logger.LogInformation("User {UserId} updated report {ReportId}", userContext.GetCurrentUserId(), report.Id);
        }

        var savedColumns = request.Columns
            .Where(column => !string.IsNullOrWhiteSpace(column.Name))
            .Select((column, index) => new ReportColumn
            {
                ReportId = report.Id,
                Name = column.Name.Trim(),
                Alias = column.Alias.Trim(),
                IsLiteral = column.IsLiteral,
                IsHidden = column.IsHidden,
                Format = column.Format.Trim(),
                EnumerationId = column.EnumerationId > 0 ? column.EnumerationId : null,
                TransformFunction = column.TransformFunction.Trim(),
                Ordinal = index,
            })
            .ToList();

        await columns.InsertAsync(savedColumns);

        var savedTables = request.Tables.Select((table, index) => new ReportTable
        {
            ReportId = report.Id,
            Name = table.Name.Trim(),
            ParentTable = table.ParentTable.Trim(),
            PrimaryKeyColumn = table.PrimaryKey.Trim(),
            ForeignKeyColumn = table.ForeignKey.Trim(),
            JoinType = table.JoinType,
            Ordinal = index,
        }).ToList();

        await tables.InsertAsync(savedTables);

        var savedSorts = request.Sorts
            .Where(sort => !string.IsNullOrWhiteSpace(sort.ColumnName))
            .Select((sort, index) => new ReportSorting
            {
                ReportId = report.Id,
                ColumnName = sort.ColumnName.Trim(),
                SortDirection = sort.Direction,
                Ordinal = index,
            })
            .ToList();

        if (savedSorts.Count > 0)
        {
            await sortings.InsertAsync(savedSorts);
        }

        var deniedUserIds = request.DeniedUserIds
            .Where(userId => !string.IsNullOrWhiteSpace(userId))
            .Select(userId => userId.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (deniedUserIds.Count > 0)
        {
            await blacklist.InsertAsync(deniedUserIds.Select(userId => new ReportUserBlacklistEntry
            {
                ReportId = report.Id,
                UserId = userId,
            }));
        }

        return Result.Success(ToDefinition(report, savedTables, savedColumns, savedSorts, deniedUserIds));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await access.CanManageAsync())
        {
            return Result.Forbidden();
        }

        await columns.DeleteAsync(column => column.ReportId == id);
        await tables.DeleteAsync(table => table.ReportId == id);
        await sortings.DeleteAsync(sort => sort.ReportId == id);
        await blacklist.DeleteAsync(entry => entry.ReportId == id);
        int deleted = await reports.DeleteAsync(report => report.Id == id);
        if (deleted == 0)
        {
            return Result.NotFound();
        }

        logger.LogInformation("User {UserId} deleted report {ReportId}", userContext.GetCurrentUserId(), id);
        return Result.Success();
    }

    public async Task<Result<ReportRunSetup>> GetRunSetupAsync(int id, CancellationToken cancellationToken = default)
    {
        var definition = await GetAsync(id, cancellationToken);
        if (!definition.IsSuccess)
        {
            return definition.Map(_ => new ReportRunSetup(string.Empty, new QueryBuilderConfig(), new QueryGroup()));
        }

        var report = definition.Value;
        if (!report.Enabled && !await access.CanManageAsync())
        {
            return Result<ReportRunSetup>.Forbidden();
        }

        var schemaResult = await schema.GetTablesAsync(report.DataSourceId, cancellationToken);
        if (!schemaResult.IsSuccess)
        {
            return schemaResult.Status == ResultStatus.Forbidden
                ? Result<ReportRunSetup>.Forbidden()
                : Result<ReportRunSetup>.Error(new ErrorList(schemaResult.Errors));
        }

        var offered = ColumnsFor(schemaResult.Value);
        var filterColumns = offered
            .Where(item => report.Columns.Any(column => !column.IsLiteral && string.Equals(column.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var rules = ReportRules.ForRunner(report.RulesJson, report.Columns);
        var config = ReportRules.Picker(Filters(filterColumns) with { AllowEmpty = true }, rules, report.Columns);
        return Result.Success(new ReportRunSetup(report.Name, config, rules));
    }

    public async Task<Result<ReportResultDto>> RunAsync(
        int id,
        bool preview,
        string? rulesJson = null,
        CancellationToken cancellationToken = default)
    {
        var definition = await GetAsync(id, cancellationToken);
        if (!definition.IsSuccess)
        {
            return definition.Map(_ => new ReportResultDto(string.Empty, [], [], string.Empty, preview, null));
        }

        var report = definition.Value;
        if (!report.Enabled && !await access.CanManageAsync())
        {
            return Result<ReportResultDto>.Forbidden();
        }

        var source = await dataSources.FindOneAsync(report.DataSourceId);
        if (source is null)
        {
            return Result<ReportResultDto>.Error("The data source is missing.");
        }

        var schemaResult = await schema.GetTablesAsync(source.Id, cancellationToken);
        if (!schemaResult.IsSuccess)
        {
            return schemaResult.Status == ResultStatus.Forbidden
                ? Result<ReportResultDto>.Forbidden()
                : Result<ReportResultDto>.Error(new ErrorList(schemaResult.Errors));
        }

        var tableInputs = report.Tables.Select(table => new ReportTableInput
        {
            Name = table.Name,
            ParentTable = table.ParentTable,
            PrimaryKey = table.PrimaryKey,
            ForeignKey = table.ForeignKey,
            JoinType = table.JoinType,
        }).ToList();

        string? joinError = ReportSql.ValidateJoins(tableInputs);
        if (joinError is not null)
        {
            return Result<ReportResultDto>.Invalid(new ValidationError(joinError));
        }

        var schemaTables = tableInputs
            .Select(table => schemaResult.Value.FirstOrDefault(item => string.Equals(item.Name, table.Name, StringComparison.OrdinalIgnoreCase)))
            .Where(item => item is not null)
            .Cast<SchemaTableDto>()
            .ToList();

        if (schemaTables.Count != tableInputs.Count)
        {
            return Result<ReportResultDto>.Error("A selected table was not found.");
        }

        var offered = ColumnsFor(schemaTables);
        var columnInputs = report.Columns.Select(column => new ReportColumnInput
        {
            Name = column.Name,
            Alias = column.Alias,
            IsLiteral = column.IsLiteral,
            IsHidden = column.IsHidden,
            Format = column.Format,
            EnumerationId = column.EnumerationId ?? 0,
            TransformFunction = column.TransformFunction,
        }).ToList();

        bool known(ReportColumnInput column) =>
            column.IsLiteral || offered.Any(item => string.Equals(item.Name, column.Name, StringComparison.OrdinalIgnoreCase));

        if (columnInputs.Where(column => !column.IsHidden).All(column => !known(column)))
        {
            return Result<ReportResultDto>.Error("None of the saved columns are on the selected tables anymore.");
        }

        var filterColumns = offered
            .Where(item => columnInputs.Any(column => !column.IsLiteral && string.Equals(column.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var config = Filters(filterColumns) with { AllowEmpty = true };
        string effectiveRules = ReportRules.Enforce(report.RulesJson, rulesJson, ReportRules.AdminFields(report.Columns));
        var where = SqlGenerator.Generate(effectiveRules, config, new SqlGeneratorOptions
        {
            Dialect = ReportSql.Dialect(source.Provider),
        });

        if (!where.IsSuccess)
        {
            return Result<ReportResultDto>.Invalid(where.ValidationErrors);
        }

        string master = tableInputs.First(table => string.IsNullOrWhiteSpace(table.ParentTable)).Name;
        var sortInputs = report.Sorts.Select(sort => new ReportSortInput
        {
            ColumnName = sort.ColumnName,
            Direction = sort.Direction,
        }).ToList();
        var visible = columnInputs.Where(column => !column.IsHidden && known(column)).ToList();
        int? limit = preview
            ? Math.Min(report.RowLimit is > 0 ? report.RowLimit.Value : Constants.PreviewRows, Constants.PreviewRows)
            : report.RowLimit;
        string sql = ReportSql.Select(
            ReportSql.Dialect(source.Provider),
            master,
            ReportSql.OrderedJoins(tableInputs),
            visible,
            where.Value,
            sortInputs,
            report.IsDistinct,
            limit);

        try
        {
            using var connection = ReportSql.Open(source.Provider, source.ConnectionString);
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            ReportSql.AddParameters(command, where.Value);

            var group = await groups.FindOneAsync(report.GroupId);
            var enumerationLookup = await LoadEnumerationsAsync(visible, cancellationToken);
            var headers = new List<string>();
            var rows = new List<IReadOnlyList<string?>>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var cells = new List<ReportPresentation.Cell>();
                for (int index = 0; index < visible.Count && index < reader.FieldCount; index++)
                {
                    object? raw = reader.IsDBNull(index) ? null : reader.GetValue(index);
                    enumerationLookup.TryGetValue(visible[index].EnumerationId, out EnumerationDto? enumeration);
                    cells.AddRange(ReportPresentation.Present(raw, visible[index], report.EnumerationHandling, enumeration, group?.TimeZoneId));
                }

                if (headers.Count == 0)
                {
                    headers.AddRange(cells.Select(cell => cell.Header));
                }

                rows.Add(cells.Select(cell => cell.Value).ToList());
            }

            if (headers.Count == 0)
            {
                headers.AddRange(PlannedHeaders(visible, report.EnumerationHandling, enumerationLookup));
            }

            return Result.Success(new ReportResultDto(report.Name, headers, rows, sql, preview, limit));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Report {ReportId} failed", id);
            return Result<ReportResultDto>.Error(ex.Message);
        }
    }

    private async Task<Dictionary<int, EnumerationDto>> LoadEnumerationsAsync(
        IReadOnlyList<ReportColumnInput> visible,
        CancellationToken cancellationToken)
    {
        var ids = visible.Where(column => column.EnumerationId > 0).Select(column => column.EnumerationId).Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var rows = await enumerations.FindAsync(new SearchOptions<Enumeration>
        {
            Query = item => ids.Contains(item.Id),
            CancellationToken = cancellationToken,
        });

        return rows.ToDictionary(
            item => item.Id,
            item => new EnumerationDto(item.Id, item.Name, item.IsBitFlags, EnumerationValues.Deserialize(item.Values)));
    }

    private static IEnumerable<string> PlannedHeaders(
        IReadOnlyList<ReportColumnInput> visible,
        EnumerationHandling handling,
        IReadOnlyDictionary<int, EnumerationDto> enumerationLookup)
    {
        foreach (var column in visible)
        {
            string header = string.IsNullOrWhiteSpace(column.Alias) ? column.Name : column.Alias;
            enumerationLookup.TryGetValue(column.EnumerationId, out EnumerationDto? enumeration);
            if (enumeration is not null && handling == EnumerationHandling.ShowBoth)
            {
                yield return header + " name";
            }

            yield return header;
        }
    }

    /// <summary>Every column of the chosen tables, named <c>Table.Column</c> the way the old wizard did.</summary>
    public static IReadOnlyList<SchemaColumnDto> ColumnsFor(IReadOnlyList<SchemaTableDto> tables) =>
        tables
            .SelectMany(table => table.Columns.Select(column => new SchemaColumnDto(
                table.Name + "." + column.Name,
                column.DataType)))
            .ToList();

    public QueryBuilderConfig Filters(IReadOnlyList<SchemaColumnDto> columns) => new()
    {
        Filters = columns.Select(column => new QueryFilter
        {
            Id = column.Name,
            Label = column.Name,
            Type = ReportSql.ValueType(column.DataType),
        }).ToList(),
    };

    private static ReportDefinitionDto ToDefinition(
        Report report,
        IEnumerable<ReportTable> tableRows,
        IEnumerable<ReportColumn> selected,
        IEnumerable<ReportSorting> sortRows,
        IReadOnlyList<string> deniedUserIds)
    {
        var tableDtos = tableRows.OrderBy(table => table.Ordinal).Select(table => new ReportTableDto(
            table.Name,
            table.ParentTable,
            table.PrimaryKeyColumn,
            table.ForeignKeyColumn,
            table.JoinType,
            table.Ordinal)).ToList();

        if (tableDtos.Count == 0)
        {
            tableDtos.Add(new ReportTableDto(report.TableName, string.Empty, string.Empty, string.Empty, ReportJoinType.InnerJoin, 0));
        }

        return new ReportDefinitionDto(
            report.Id,
            report.Name,
            report.GroupId,
            report.DataSourceId,
            report.TableName,
            tableDtos,
            report.IsDistinct,
            report.RowLimit,
            report.Enabled,
            report.EmailEnabled,
            report.EnumerationHandling,
            report.RulesJson,
            selected.OrderBy(column => column.Ordinal).Select(column => new ReportColumnDto(
                column.Name,
                column.Alias,
                column.IsLiteral,
                column.IsHidden,
                column.Format,
                column.EnumerationId,
                column.TransformFunction,
                column.Ordinal)).ToList(),
            sortRows.OrderBy(sort => sort.Ordinal).Select(sort => new ReportSortDto(sort.ColumnName, sort.SortDirection, sort.Ordinal)).ToList(),
            deniedUserIds);
    }

    private async Task<bool> MayViewAsync(Report report, CancellationToken cancellationToken)
    {
        if (!report.Enabled && !await access.CanManageAsync())
        {
            return false;
        }

        var roles = await groupRoles.FindAsync(new SearchOptions<ReportGroupRole>
        {
            Query = role => role.ReportGroupId == report.GroupId,
            CancellationToken = cancellationToken,
        });
        var denied = await blacklist.FindAsync(new SearchOptions<ReportUserBlacklistEntry>
        {
            Query = entry => entry.ReportId == report.Id,
            CancellationToken = cancellationToken,
        });
        return await access.CanRunAsync(roles.Select(role => role.RoleId).ToList(), denied.Select(entry => entry.UserId).ToList());
    }

    private async Task<Dictionary<int, IReadOnlyList<string>>> RolesByGroupAsync(CancellationToken cancellationToken)
    {
        var rows = await groupRoles.FindAsync(new SearchOptions<ReportGroupRole> { CancellationToken = cancellationToken });
        return rows
            .GroupBy(role => role.ReportGroupId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.Select(role => role.RoleId).ToList());
    }

    private async Task<Dictionary<int, IReadOnlyList<string>>> DeniedByReportAsync(CancellationToken cancellationToken)
    {
        var rows = await blacklist.FindAsync(new SearchOptions<ReportUserBlacklistEntry> { CancellationToken = cancellationToken });
        return rows
            .GroupBy(entry => entry.ReportId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.Select(entry => entry.UserId).ToList());
    }
}
