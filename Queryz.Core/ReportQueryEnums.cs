namespace Queryz;

/// <summary>Matches the join kinds the old wizard offered.</summary>
public enum ReportJoinType : byte
{
    InnerJoin = 0,
    OuterJoin = 1,
    LeftJoin = 2,
    RightJoin = 3,
}

public enum ReportSortDirection : byte
{
    Ascending = 0,
    Descending = 1,
}

/// <summary>How a column that uses an enumeration is shown when the report runs.</summary>
public enum EnumerationHandling : byte
{
    ShowText = 0,
    ShowNumericValue = 1,
    ShowBoth = 2,
}

/// <summary>The download formats the old run page offered.</summary>
public enum DownloadFileFormat : byte
{
    Delimited = 0,
    Xlsx = 1,
}

public enum DownloadFileDelimiter : byte
{
    Comma = 0,
    Tab = 1,
    VerticalBar = 2,
    Semicolon = 3,
}
