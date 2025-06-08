namespace Queryz.Infrastructure;

public class QueryzOptions
{
    public string AppName { get; set; } = "Queryz";

    public string Layout { get; set; } = "~/Views/Shared/_QueryzDefaultLayout.cshtml";

    public int GridPageSize { get; set; } = 10;

    public string FooterText { get; set; } = @$"&copy; {DateTime.Now.Year} - Queryz";
}