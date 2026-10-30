using System.Text.Json;

namespace Queryz.Services.Reports;

public static class EnumerationValues
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(IEnumerable<EnumerationValueDto> values) =>
        JsonSerializer.Serialize(values, Json);

    public static IReadOnlyList<EnumerationValueDto> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<EnumerationValueDto>>(json, Json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
