using System.ComponentModel.DataAnnotations;

namespace Queryz.Models;

public sealed record DataSourceDto(int Id, string Name, ReportProvider Provider);

public sealed record SaveDataSourceRequest
{
    [Required]
    [MaxLength(Constants.MaxLengths.Name)]
    public string Name { get; set; } = string.Empty;

    public ReportProvider Provider { get; set; } = ReportProvider.Sqlite;

    /// <summary>Set by the service when the form leaves the password blank on an existing source.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    public DataSourceConnectionRequest Connection { get; set; } = new();
}

public sealed record DataSourceConnectionRequest
{
    public string Server { get; set; } = string.Empty;

    public int? Port { get; set; }

    public string Database { get; set; } = string.Empty;

    public string Schema { get; set; } = string.Empty;

    public bool IntegratedSecurity { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;
}

public sealed record DataSourceDetailsDto(
    int Id,
    string Name,
    ReportProvider Provider,
    DataSourceConnectionRequest Connection,
    bool HasPassword);

public sealed record ReportGroupDto(int Id, string Name, string TimeZoneId, IReadOnlyList<string> RoleIds);

public sealed record SaveReportGroupRequest
{
    [Required]
    [MaxLength(Constants.MaxLengths.Name)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(Constants.MaxLengths.TimeZoneId)]
    public string TimeZoneId { get; set; } = string.Empty;

    public List<string> RoleIds { get; set; } = [];
}

public sealed record EnumerationValueDto(int Id, string Name);

public sealed record EnumerationDto(int Id, string Name, bool IsBitFlags, IReadOnlyList<EnumerationValueDto> Values);

public sealed class EnumerationValueInput
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class SaveEnumerationRequest
{
    [Required]
    [MaxLength(Constants.MaxLengths.Name)]
    public string Name { get; set; } = string.Empty;

    public bool IsBitFlags { get; set; }

    public List<EnumerationValueInput> Values { get; set; } = [];
}

public sealed record ReportSummaryDto(int Id, string Name, string GroupName, string DataSourceName, bool Enabled);

public sealed record ReportColumnDto(
    string Name,
    string Alias,
    bool IsLiteral,
    bool IsHidden,
    string Format,
    int? EnumerationId,
    string TransformFunction,
    int Ordinal);

public sealed record ReportTableDto(
    string Name,
    string ParentTable,
    string PrimaryKey,
    string ForeignKey,
    ReportJoinType JoinType,
    int Ordinal);

public sealed record ReportSortDto(string ColumnName, ReportSortDirection Direction, int Ordinal);

public sealed record ReportDefinitionDto(
    int Id,
    string Name,
    int GroupId,
    int DataSourceId,
    string TableName,
    IReadOnlyList<ReportTableDto> Tables,
    bool IsDistinct,
    int? RowLimit,
    bool Enabled,
    bool EmailEnabled,
    EnumerationHandling EnumerationHandling,
    string RulesJson,
    IReadOnlyList<ReportColumnDto> Columns,
    IReadOnlyList<ReportSortDto> Sorts,
    IReadOnlyList<string> DeniedUserIds);

public sealed class ReportColumnInput
{
    public string Name { get; set; } = string.Empty;

    public string Alias { get; set; } = string.Empty;

    public bool IsLiteral { get; set; }

    public bool IsHidden { get; set; }

    public string Format { get; set; } = string.Empty;

    /// <summary>Zero means the column does not use an enumeration.</summary>
    public int EnumerationId { get; set; }

    public string TransformFunction { get; set; } = string.Empty;
}

public sealed class ReportTableInput
{
    public string Name { get; set; } = string.Empty;

    public string ParentTable { get; set; } = string.Empty;

    public string PrimaryKey { get; set; } = string.Empty;

    public string ForeignKey { get; set; } = string.Empty;

    public ReportJoinType JoinType { get; set; } = ReportJoinType.InnerJoin;
}

public sealed class ReportSortInput
{
    public string ColumnName { get; set; } = string.Empty;

    public ReportSortDirection Direction { get; set; } = ReportSortDirection.Ascending;
}

public sealed record SaveReportRequest
{
    [Required]
    [MaxLength(Constants.MaxLengths.Name)]
    public string Name { get; set; } = string.Empty;

    public int GroupId { get; set; }

    public int DataSourceId { get; set; }

    [MaxLength(Constants.MaxLengths.ColumnName)]
    public string TableName { get; set; } = string.Empty;

    public List<ReportTableInput> Tables { get; set; } = [];

    public bool IsDistinct { get; set; }

    public int? RowLimit { get; set; }

    public bool Enabled { get; set; } = true;

    public bool EmailEnabled { get; set; }

    public EnumerationHandling EnumerationHandling { get; set; } = EnumerationHandling.ShowText;

    public string RulesJson { get; set; } = string.Empty;

    public List<string> DeniedUserIds { get; set; } = [];

    public List<ReportColumnInput> Columns { get; set; } = [];

    public List<ReportSortInput> Sorts { get; set; } = [];
}

public sealed record SchemaColumnDto(string Name, string DataType);

public sealed record SchemaTableDto(string Name, IReadOnlyList<SchemaColumnDto> Columns);

public sealed record ReportResultDto(
    string Name,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string?>> Rows,
    string Sql,
    bool IsPreview,
    int? AppliedLimit);

public sealed class ReportDownloadRequest
{
    public DownloadFileFormat FileFormat { get; set; } = DownloadFileFormat.Delimited;

    public DownloadFileDelimiter Delimiter { get; set; } = DownloadFileDelimiter.Comma;

    public bool AlwaysEnquote { get; set; }

    public bool OutputColumnNames { get; set; }

    /// <summary>The filters the runner set. Hidden conditions are taken from the saved report.</summary>
    public string? RulesJson { get; set; }
}
