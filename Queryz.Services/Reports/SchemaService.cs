using System.Data;

namespace Queryz.Services.Reports;

public interface ISchemaService
{
    Task<Result<IReadOnlyList<SchemaTableDto>>> GetTablesAsync(int dataSourceId, CancellationToken cancellationToken = default);
}

public sealed class SchemaService(
    ILogger<SchemaService> logger,
    IRepository<DataSource> dataSources) : ISchemaService
{
    public async Task<Result<IReadOnlyList<SchemaTableDto>>> GetTablesAsync(
        int dataSourceId,
        CancellationToken cancellationToken = default)
    {
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

    private static string?[] SchemaRestrictions(string? schema, string table) =>
        [null, string.IsNullOrWhiteSpace(schema) ? null : schema, table, null];

    private static string? Column(DataRow row, string name) =>
        row.Table.Columns.Contains(name) && row[name] != DBNull.Value ? Convert.ToString(row[name]) : null;
}
