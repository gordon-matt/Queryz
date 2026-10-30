using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Queryz.Services.Reports;

public static class ReportExport
{
    public static (byte[] Bytes, string ContentType, string FileName) Create(string reportName, ReportResultDto result, ReportDownloadRequest request)
    {
        string stamp = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss", CultureInfo.InvariantCulture);
        if (request.FileFormat == DownloadFileFormat.Xlsx)
        {
            string name = reportName.Replace("[", "(").Replace("]", ")");
            return (ToXlsx(result), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{SafeFileName(name)}_{stamp}.xlsx");
        }

        string separator = request.Delimiter switch
        {
            DownloadFileDelimiter.Tab => "\t",
            DownloadFileDelimiter.VerticalBar => "|",
            DownloadFileDelimiter.Semicolon => ";",
            _ => ",",
        };
        string extension = request.Delimiter switch
        {
            DownloadFileDelimiter.Tab => "tsv",
            DownloadFileDelimiter.Comma => "csv",
            _ => "txt",
        };
        string contentType = request.Delimiter switch
        {
            DownloadFileDelimiter.Tab => "text/tab-separated-values",
            DownloadFileDelimiter.Comma => "text/csv",
            _ => "text/plain",
        };

        string text = ToDelimited(result, separator, request.OutputColumnNames, request.AlwaysEnquote);
        return (Encoding.UTF8.GetBytes(text), contentType, $"{SafeFileName(reportName)}_{stamp}.{extension}");
    }

    public static string ToDelimited(ReportResultDto result, string delimiter, bool outputColumnNames, bool alwaysEnquote)
    {
        var text = new StringBuilder();
        if (outputColumnNames)
        {
            WriteRow(text, result.Columns, delimiter, alwaysEnquote);
        }

        foreach (var row in result.Rows)
        {
            WriteRow(text, row, delimiter, alwaysEnquote);
        }

        return text.ToString();
    }

    public static byte[] ToXlsx(ReportResultDto result)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(zip, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);
            WriteEntry(zip, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            WriteEntry(zip, "xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets><sheet name="Report" sheetId="1" r:id="rId1"/></sheets>
                </workbook>
                """);
            WriteEntry(zip, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);

            var sheet = zip.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Optimal);
            using var writer = new StreamWriter(sheet.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            writer.Write("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>""");
            WriteSheetRow(writer, 1, result.Columns);
            for (int index = 0; index < result.Rows.Count; index++)
            {
                WriteSheetRow(writer, index + 2, result.Rows[index]);
            }

            writer.Write("</sheetData></worksheet>");
        }

        return stream.ToArray();
    }

    private static void WriteRow(StringBuilder text, IReadOnlyList<string?> cells, string delimiter, bool alwaysEnquote)
    {
        for (int index = 0; index < cells.Count; index++)
        {
            if (index > 0)
            {
                text.Append(delimiter);
            }

            string value = (cells[index] ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal);
            if (alwaysEnquote || value.Contains(delimiter, StringComparison.Ordinal))
            {
                text.Append('"').Append(value).Append('"');
            }
            else
            {
                text.Append(NewLines().Replace(value, " "));
            }
        }

        text.Append(Environment.NewLine);
    }

    private static void WriteSheetRow(StreamWriter writer, int rowNumber, IReadOnlyList<string?> cells)
    {
        writer.Write("<row r=\"");
        writer.Write(rowNumber);
        writer.Write("\">");
        for (int index = 0; index < cells.Count; index++)
        {
            writer.Write("<c r=\"");
            writer.Write(ColumnLetter(index));
            writer.Write(rowNumber);
            writer.Write("\" t=\"inlineStr\"><is><t xml:space=\"preserve\">");
            writer.Write(EscapeXml(cells[index]));
            writer.Write("</t></is></c>");
        }

        writer.Write("</row>");
    }

    private static string ColumnLetter(int index)
    {
        var letters = new StringBuilder();
        index++;
        while (index > 0)
        {
            index--;
            letters.Insert(0, (char)('A' + (index % 26)));
            index /= 26;
        }

        return letters.ToString();
    }

    private static string EscapeXml(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var text = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            switch (character)
            {
                case '<': text.Append("&lt;"); break;
                case '>': text.Append("&gt;"); break;
                case '&': text.Append("&amp;"); break;
                default:
                    if (character is '\t' or '\n' or '\r' || character >= ' ')
                    {
                        text.Append(character);
                    }

                    break;
            }
        }

        return text.ToString();
    }

    private static void WriteEntry(ZipArchive zip, string name, string contents)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(contents);
    }

    private static string SafeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new StringBuilder(name.Length);
        foreach (char character in name)
        {
            safe.Append(invalid.Contains(character) ? '_' : character);
        }

        string trimmed = safe.ToString().Trim();
        return trimmed.Length == 0 ? "Report" : trimmed;
    }

    private static Regex NewLines() => NewLinePattern;

    private static readonly Regex NewLinePattern = new(@"\t|\r|\n", RegexOptions.Compiled);
}
