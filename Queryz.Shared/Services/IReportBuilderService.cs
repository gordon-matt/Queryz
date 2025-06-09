using System.Data;
using System.Globalization;
using System.Text;
using Extenso.Data.Common;
using Microsoft.Data.SqlClient;
using Queryz.Data.Entities;
using Queryz.Data.TransformFunctions;
using Queryz.Extensions;
using Queryz.Models;

namespace Queryz.Services;

public interface IReportBuilderService
{
    DataTable ExecuteReport(Report report);
}

public class ReportBuilderService : IReportBuilderService
{
    private readonly IEnumerationService enumerationService;
    private readonly IEnumerable<ITransformFunction> transformFunctions;

    public ReportBuilderService(
        IEnumerationService enumerationService,
        IEnumerable<ITransformFunction> transformFunctions)
    {
        this.enumerationService = enumerationService;
        this.transformFunctions = transformFunctions;
    }

    public DataTable ExecuteReport(Report report)
    {
        var dataTable = new DataTable(report.Name);

        using (var connection = report.DataSource.DataProvider.GetConnection(report.DataSource.ConnectionString))
        {
            var query = report.DataSource.GetSelectQueryBuilder();

            #region SELECT

            var columns = report.Columns
                .Where(x => !x.IsHidden)
                .OrderBy(x => x.Ordinal)
                .ToHashSet();

            foreach (var column in columns)
            {
                if (column.IsLiteral)
                {
                    query.Select(new SqlLiteral($@"{column.Name} AS ""{column.Alias}"""));
                }
                else
                {
                    string tableName = column.Name.LeftOfLastIndexOf('.');
                    string columnName = column.Name.RightOfLastIndexOf('.');

                    query.SelectAs(tableName, columnName, column.Alias);
                }
            }

            #endregion SELECT

            #region FROM

            var masterTable = report.Tables.FirstOrDefault(x => x.IsEmpty);

            if (masterTable == null)
            {
                throw new ArgumentException("Please select one and ONLY one master table.");
            }

            query = query.From(masterTable.Name);

            #endregion FROM

            #region TOP / LIMIT

            if (report.RowLimit.HasValue && report.RowLimit.Value > 0)
            {
                query = query.Take(report.RowLimit.Value);
            }

            #endregion TOP / LIMIT

            #region DISTINCT

            if (report.IsDistinct)
            {
                query.Distinct();

                var selectedColumns = report.Columns.Select(x => x.Name).ToList();
                foreach (var sorting in report.Sortings.OrderBy(x => x.Ordinal))
                {
                    // If DISTINCT is specified, then columns specified in ORDER BY must be in SELECT also
                    if (!selectedColumns.Contains(sorting.ColumnName))
                    {
                        query.Select(sorting.ColumnName);
                    }
                }
            }

            #endregion DISTINCT

            #region JOINS

            // Keep track of which tables have already been added to the join
            var list = new List<string> { masterTable.Name };

            var relationships = report.Tables.Where(x => !x.IsEmpty).ToList();

            while (relationships.Count > 0)
            {
                for (int i = 0; i < relationships.Count; i++)
                {
                    var join = relationships.ElementAt(i);

                    if (!list.Contains(join.ParentTable))
                    {
                        continue;
                    }

                    query.Join(join.JoinType, join.Name, join.ForeignKeyColumn, ComparisonOperator.EqualTo, join.ParentTable, join.PrimaryKeyColumn);
                    list.Add(join.Name);
                    relationships.Remove(join);
                }
            }

            #endregion JOINS

            #region WHERE

            if (!string.IsNullOrEmpty(report.Filters))
            {
                query = query.Where(report.Filters);
            }

            #endregion WHERE

            #region ORDER BY

            foreach (var sorting in report.Sortings.OrderBy(x => x.Ordinal))
            {
                if (sorting.ColumnName.Contains('.'))
                {
                    string tableName = sorting.ColumnName.LeftOfLastIndexOf('.');
                    string columnName = sorting.ColumnName.RightOfLastIndexOf('.');
                    query = query.OrderBy(tableName, columnName, sorting.SortDirection);
                }
                else
                {
                    query = query.OrderBy(sorting.ColumnName, sorting.SortDirection);
                }
            }

            #endregion ORDER BY

            using (var command = connection.CreateCommand())
            {
                command.CommandTimeout = 300;
                command.CommandType = CommandType.Text;

                if (connection is SqlConnection)
                {
                    // customer requested this transaction be set to prevent SQL implicit transaction, which blocks writes
                    command.CommandText = $@"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED BEGIN TRANSACTION; {query.BuildQuery()} COMMIT TRANSACTION;";
                }
                else
                {
                    command.CommandText = query.BuildQuery();
                }
                try
                {
                    using var adapter = connection.GetDbProviderFactory().CreateDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dataTable);
                }
                catch (Exception x)
                {
                    throw new ReportingException($"Error when executing report query: {command.CommandText}", x);
                }
            }
            connection.Close();
        }

        foreach (var column in report.Columns)
        {
            if (column.IsHidden)
            {
                continue;
            }

            string columnName = column.Name.RightOfLastIndexOf('.');
            if (!string.IsNullOrEmpty(column.Alias) && dataTable.Columns.Contains(column.Alias))
            {
                columnName = column.Alias;
            }

            #region Apply Transform Functions

            if (!string.IsNullOrEmpty(column.TransformFunction))
            {
                var transformFunction = transformFunctions.FirstOrDefault(x => x.Name == column.TransformFunction);
                if (transformFunction != null)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        object value = row[columnName];
                        var result = transformFunction.Transform(value, report);
                        row[columnName] = result;
                    }
                }
            }

            #endregion Apply Transform Functions

            #region Handle Enumerations

            if (column.EnumerationId.HasValue)
            {
                // NOTES:
                // For EnumerationHandling.ShowNumericValue, we skip this (do nothing)
                // For EnumerationHandling.ShowBoth, we use the code below
                // For EnumerationHandling.ShowText, we also use the code below, but then delete the original column

                if (report.EnumerationHandling != EnumerationHandling.ShowNumericValue)
                {
                    var enumeration = enumerationService.FindOne(column.EnumerationId.Value);
                    var enumValues = enumeration.Values.JsonDeserialize<IdNamePair<int>[]>();

                    // First check if the data type is correct (in case the user set a string column, or other type, to be a lookup column).
                    //  Only integer columns should be lookup columns (int, short, byte, uint, ushort, sbyte)
                    var dataColumn = dataTable.Columns[columnName];
                    if (dataColumn.DataType.In(typeof(int), typeof(short), typeof(byte), typeof(uint), typeof(ushort), typeof(sbyte)))
                    {
                        string newColumnName = $"{columnName}_Name";
                        var newColumn = new DataColumn
                        {
                            ColumnName = newColumnName,
                            DataType = typeof(string)
                        };
                        dataTable.Columns.Add(newColumn);
                        newColumn.SetOrdinal(dataColumn.Ordinal);

                        foreach (DataRow row in dataTable.Rows)
                        {
                            object value = row[columnName];
                            if (value != DBNull.Value)
                            {
                                int intValue = int.Parse(value.ToString());

                                if (enumeration.IsBitFlags)
                                {
                                    var flags = GetFlags(enumValues, intValue);
                                    var sb = new StringBuilder();

                                    foreach (int flag in flags)
                                    {
                                        var match = enumValues.FirstOrDefault(x => x.Id == flag);
                                        if (match != null)
                                        {
                                            sb.Append($"{match.Name} + ");
                                        }
                                    }

                                    sb = sb.Remove(sb.Length - 3, 3);
                                    row[newColumnName] = sb.ToString();
                                }
                                else
                                {
                                    var match = enumValues.FirstOrDefault(x => x.Id == intValue);
                                    if (match != null)
                                    {
                                        row[newColumnName] = match.Name;
                                    }
                                }
                            }
                        }

                        if (report.EnumerationHandling == EnumerationHandling.ShowText)
                        {
                            dataTable.Columns.Remove(dataColumn);
                            newColumn.ColumnName = newColumn.ColumnName.Replace("_Name", string.Empty);
                        }
                    }
                }
            }

            #endregion Handle Enumerations

            #region Format

            else
            {
                if (!string.IsNullOrEmpty(column.Format))
                {
                    var dataColumn = dataTable.Columns[columnName];

                    if (dataColumn.AllowDBNull)
                    {
                        switch (dataColumn.DataType.Name)
                        {
                            case "DateTime": dataColumn.ChangeDataType<DateTime?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "Int16": dataColumn.ChangeDataType<short?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "Int32": dataColumn.ChangeDataType<int?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "Int64": dataColumn.ChangeDataType<long?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "UInt16": dataColumn.ChangeDataType<ushort?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "UInt32": dataColumn.ChangeDataType<uint?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "UInt64": dataColumn.ChangeDataType<ulong?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "Byte": dataColumn.ChangeDataType<byte?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "SByte": dataColumn.ChangeDataType<sbyte?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "Single": dataColumn.ChangeDataType<float?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "Double": dataColumn.ChangeDataType<double?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            case "Decimal": dataColumn.ChangeDataType<decimal?, string>(x => x.HasValue ? x.Value.ToString(column.Format, CultureInfo.InvariantCulture) : null); break;
                            default: break;
                        }
                    }
                    else
                    {
                        switch (dataColumn.DataType.Name)
                        {
                            case "DateTime": dataColumn.ChangeDataType<DateTime, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "Int16": dataColumn.ChangeDataType<short, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "Int32": dataColumn.ChangeDataType<int, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "Int64": dataColumn.ChangeDataType<long, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "UInt16": dataColumn.ChangeDataType<ushort, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "UInt32": dataColumn.ChangeDataType<uint, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "UInt64": dataColumn.ChangeDataType<ulong, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "Byte": dataColumn.ChangeDataType<byte, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "SByte": dataColumn.ChangeDataType<sbyte, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "Single": dataColumn.ChangeDataType<float, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "Double": dataColumn.ChangeDataType<double, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            case "Decimal": dataColumn.ChangeDataType<decimal, string>(x => x.ToString(column.Format, CultureInfo.InvariantCulture)); break;
                            default: break;
                        }
                    }
                }
            }

            #endregion Format
        }

        dataTable.AcceptChanges();

        return dataTable;
    }

    public static IEnumerable<int> GetFlags(IdNamePair<int>[] enumValues, int source)
    {
        ulong flag = 1;
        foreach (int value in enumValues.Select(x => x.Id))
        {
            ulong bits = Convert.ToUInt64(value);
            while (flag < bits)
            {
                flag <<= 1;
            }

            if (flag == bits && HasFlag(source, value))
            {
                yield return value;
            }
        }
    }

    public static bool HasFlag(int source, int flag)
    {
        ulong uFlag = (ulong)flag;
        ulong uSource = (ulong)source;
        return (uSource & uFlag) == uFlag;
    }
}