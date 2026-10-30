using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using QueryKrafter.Models;
using QueryKrafter.Sql;

namespace Queryz.Services.Reports;

public static class ReportSql
{
    public static SqlDialect Dialect(ReportProvider provider) => provider switch
    {
        ReportProvider.SqlServer => SqlDialect.SqlServer,
        ReportProvider.PostgreSql => SqlDialect.PostgreSql,
        ReportProvider.MySql => SqlDialect.MySql,
        ReportProvider.Sqlite => SqlDialect.Sqlite,
        _ => SqlDialect.Ansi,
    };

    public static DbConnection Open(ReportProvider provider, string connectionString)
    {
        DbConnection connection = provider switch
        {
            ReportProvider.SqlServer => new SqlConnection(connectionString),
            ReportProvider.PostgreSql => new NpgsqlConnection(connectionString),
            ReportProvider.MySql => new MySqlConnection(connectionString),
            ReportProvider.Sqlite => new SqliteConnection(connectionString),
            _ => throw new InvalidOperationException($"Unsupported report provider '{provider}'."),
        };

        connection.Open();
        return connection;
    }

    public static QueryValueType ValueType(string dataType)
    {
        string type = dataType.ToLowerInvariant();

        if (type.Contains("bool", StringComparison.Ordinal) || type is "bit")
        {
            return QueryValueType.Boolean;
        }

        if (type.Contains("date", StringComparison.Ordinal) && type.Contains("time", StringComparison.Ordinal))
        {
            return QueryValueType.DateTime;
        }

        if (type.Contains("date", StringComparison.Ordinal))
        {
            return QueryValueType.Date;
        }

        if (type.Contains("time", StringComparison.Ordinal))
        {
            return QueryValueType.Time;
        }

        if (type.Contains("int", StringComparison.Ordinal) || type is "bigint" or "smallint" or "tinyint")
        {
            return QueryValueType.Integer;
        }

        if (type.Contains("real", StringComparison.Ordinal)
            || type.Contains("float", StringComparison.Ordinal)
            || type.Contains("double", StringComparison.Ordinal)
            || type.Contains("decimal", StringComparison.Ordinal)
            || type.Contains("numeric", StringComparison.Ordinal)
            || type.Contains("money", StringComparison.Ordinal))
        {
            return QueryValueType.Double;
        }

        return QueryValueType.String;
    }

    public static string? ValidateJoins(IReadOnlyList<ReportTableInput> tables)
    {
        if (tables.Count == 0)
        {
            return "Select a table.";
        }

        int masters = tables.Count(table => string.IsNullOrWhiteSpace(table.ParentTable));
        if (masters != 1)
        {
            return "Leave the parent empty on exactly one table. That table is the one the query starts from.";
        }

        foreach (var table in tables.Where(table => !string.IsNullOrWhiteSpace(table.ParentTable)))
        {
            if (string.IsNullOrWhiteSpace(table.PrimaryKey) || string.IsNullOrWhiteSpace(table.ForeignKey))
            {
                return $"Set the primary key and foreign key for {table.Name}.";
            }

            if (string.Equals(table.ParentTable, table.Name, StringComparison.OrdinalIgnoreCase))
            {
                return $"{table.Name} cannot join to itself.";
            }

            if (!tables.Any(other => string.Equals(other.Name, table.ParentTable, StringComparison.OrdinalIgnoreCase)))
            {
                return $"The parent of {table.Name} is not one of the selected tables.";
            }
        }

        var pending = tables.Where(table => !string.IsNullOrWhiteSpace(table.ParentTable)).ToList();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            tables.First(table => string.IsNullOrWhiteSpace(table.ParentTable)).Name,
        };

        while (pending.Count > 0)
        {
            var next = pending.FirstOrDefault(table => seen.Contains(table.ParentTable));
            if (next is null)
            {
                return "Every joined table must connect back to the main table.";
            }

            seen.Add(next.Name);
            pending.Remove(next);
        }

        return null;
    }

    public static IReadOnlyList<ReportTableInput> OrderedJoins(IReadOnlyList<ReportTableInput> tables)
    {
        var pending = tables.Where(table => !string.IsNullOrWhiteSpace(table.ParentTable)).ToList();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            tables.First(table => string.IsNullOrWhiteSpace(table.ParentTable)).Name,
        };
        var ordered = new List<ReportTableInput>();

        while (pending.Count > 0)
        {
            var next = pending.First(table => seen.Contains(table.ParentTable));
            ordered.Add(next);
            seen.Add(next.Name);
            pending.Remove(next);
        }

        return ordered;
    }

    public static string AliasFor(string qualifiedName)
    {
        string name = qualifiedName.Contains('.')
            ? qualifiedName[(qualifiedName.LastIndexOf('.') + 1)..]
            : qualifiedName;

        if (name.Contains(' '))
        {
            return name.Trim();
        }

        var spaced = new StringBuilder();
        for (int index = 0; index < name.Length; index++)
        {
            if (index > 0 && char.IsUpper(name[index]) && char.IsLower(name[index - 1]))
            {
                spaced.Append(' ');
            }

            spaced.Append(name[index]);
        }

        return spaced.ToString();
    }

    public static string Select(
        SqlDialect dialect,
        string masterTable,
        IReadOnlyList<ReportTableInput> joins,
        IReadOnlyList<ReportColumnInput> columns,
        SqlQuery where,
        IReadOnlyList<ReportSortInput> sorts,
        bool distinct,
        int? rowLimit)
    {
        var sql = new StringBuilder("SELECT ");

        if (distinct)
        {
            sql.Append("DISTINCT ");
        }

        if (rowLimit is > 0 && dialect == SqlDialect.SqlServer)
        {
            sql.Append("TOP (").Append(rowLimit.Value).Append(") ");
        }

        var selected = columns.Where(column => !column.IsHidden).ToList();
        var selectList = selected.Select(column => Project(dialect, column)).ToList();

        if (distinct)
        {
            foreach (var sort in sorts)
            {
                bool alreadySelected = selected.Any(column =>
                    string.Equals(column.Name, sort.ColumnName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(column.Alias, sort.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (!alreadySelected && !string.IsNullOrWhiteSpace(sort.ColumnName))
                {
                    selectList.Add(dialect.QuoteIdentifier(sort.ColumnName));
                }
            }
        }

        sql.Append(string.Join(", ", selectList));
        sql.Append(" FROM ").Append(dialect.QuoteIdentifier(masterTable));

        foreach (var join in joins)
        {
            sql.Append(' ').Append(JoinKeyword(join.JoinType)).Append(' ');
            sql.Append(dialect.QuoteIdentifier(join.Name));
            sql.Append(" ON ");
            sql.Append(dialect.QuoteIdentifier(join.ParentTable + "." + join.PrimaryKey));
            sql.Append(" = ");
            sql.Append(dialect.QuoteIdentifier(join.Name + "." + join.ForeignKey));
        }

        if (!where.IsEmpty)
        {
            sql.Append(" WHERE ").Append(where.Sql);
        }

        if (sorts.Count > 0)
        {
            sql.Append(" ORDER BY ");
            sql.Append(string.Join(", ", sorts.Select(sort =>
                dialect.QuoteIdentifier(sort.ColumnName)
                + (sort.Direction == ReportSortDirection.Descending ? " DESC" : " ASC"))));
        }

        if (rowLimit is > 0 && dialect != SqlDialect.SqlServer)
        {
            sql.Append(" LIMIT ").Append(rowLimit.Value);
        }

        return sql.ToString();
    }

    private static string Project(SqlDialect dialect, ReportColumnInput column)
    {
        string expression = column.IsLiteral ? column.Name.Trim() : dialect.QuoteIdentifier(column.Name);
        if (string.IsNullOrWhiteSpace(column.Alias))
        {
            return expression;
        }

        return expression + " AS " + dialect.QuoteIdentifier(column.Alias);
    }

    private static string JoinKeyword(ReportJoinType joinType) => joinType switch
    {
        ReportJoinType.LeftJoin => "LEFT JOIN",
        ReportJoinType.RightJoin => "RIGHT JOIN",
        ReportJoinType.OuterJoin => "FULL OUTER JOIN",
        _ => "INNER JOIN",
    };

    public static void AddParameters(DbCommand command, SqlQuery where)
    {
        foreach (var parameter in where.Parameters)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = parameter.Name;
            dbParameter.Value = parameter.Value ?? DBNull.Value;
            command.Parameters.Add(dbParameter);
        }
    }
}
