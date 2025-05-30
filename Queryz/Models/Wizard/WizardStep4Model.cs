using Extenso.Data.QueryBuilder;
using JQQueryBuilderHelpers;

namespace Queryz.Models;

public class WizardStep4Model
{
    public int ReportId { get; set; }

    public string Query { get; set; }

    public JQQueryBuilderConfig JQQueryBuilderConfig { get; set; }

    public IDictionary<string, string> JQQueryBuilderFieldIdMappings { get; set; }
}

public class FilterColumnModel
{
    public string Name { get; set; }

    public string Type { get; set; }

    public int Ordinal { get; set; }

    public string PrimaryKeyTable { get; set; }

    public string PrimaryKeyColumn { get; set; }

    /// <summary>
    /// The column in the primary key table to display instead of the ID (normally a "name" column)
    /// </summary>
    public string DisplayColumn { get; set; }

    public int? EnumerationId { get; set; }

    public EnumerationModel[] AvailableEnumerations { get; set; }
}

public class FilterModel
{
    public LogicOperator LogicOperator { get; set; }

    public dynamic Column { get; set; }

    public ComparisonOperator ComparisonOperator { get; set; }

    public string Value { get; set; }

    public IEnumerable<FilterModel> SubFilters { get; set; }

    //public static implicit operator WhereClause (FilterModel other)
    //{
    //    string[] split = (other.Column.Name as string).Split('.');
    //    string table = split[0];
    //    string column = split[1];
    //    var clause = new WhereClause(other.LogicOperator, table, column, other.ComparisonOperator, other.Value);
    //}
}