namespace Queryz.Models;

public class RunReportModel
{
    public int ReportId { get; set; }

    public string ReportName { get; set; }

    public string Query { get; set; }

    public JQQueryBuilderConfig JQQueryBuilderConfig { get; set; }

    public IDictionary<string, string> JQQueryBuilderFieldIdMappings { get; set; }
}