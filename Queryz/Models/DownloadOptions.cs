namespace Queryz.Models;

public class DownloadOptions
{
    public bool AlwaysEnquote { get; set; } = true;

    public DownloadFileDelimiter Delimiter { get; set; } = DownloadFileDelimiter.Comma;

    public DownloadFileFormat FileFormat { get; set; }

    public bool OutputColumnNames { get; set; } = true;
}