namespace Queryz.Services.Reports;

public static class ReportTransforms
{
    public const string RoundUp = "Round Up";

    public const string BeautifyJson = "Beautify JSON";

    public const string ConvertTimeZone = "Convert Time Zone";

    public static readonly IReadOnlyList<string> All = [RoundUp, BeautifyJson, ConvertTimeZone];

    public static object? Apply(object? value, string transform, string? timeZoneId)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return transform switch
        {
            RoundUp => Round(value),
            BeautifyJson => Beautify(value),
            ConvertTimeZone => Convert(value, timeZoneId),
            _ => value,
        };
    }

    private static object Round(object value) => value switch
    {
        float number => Math.Ceiling(number),
        double number => Math.Ceiling(number),
        decimal number => Math.Ceiling(number),
        _ => value,
    };

    private static object Beautify(object value)
    {
        if (value is not string text)
        {
            return value;
        }

        string trimmed = text.Trim();
        bool looksLikeJson = (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
            || (trimmed.StartsWith('[') && trimmed.EndsWith(']'));
        if (!looksLikeJson)
        {
            return value;
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(trimmed);
            return System.Text.Json.JsonSerializer.Serialize(document, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        catch (System.Text.Json.JsonException)
        {
            return value;
        }
    }

    private static object Convert(object value, string? timeZoneId)
    {
        if (value is not DateTime date || string.IsNullOrWhiteSpace(timeZoneId))
        {
            return value;
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo? zone))
        {
            return value;
        }

        DateTime utc = date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc),
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
    }
}
