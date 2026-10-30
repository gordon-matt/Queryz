using System.Globalization;
using System.Text;

namespace Queryz.Services.Reports;

/// <summary>
/// Turns a raw cell into the text the grid shows: transform, then enumeration, then format.
/// Enumeration text is applied the way the old report runner did.
/// </summary>
public static class ReportPresentation
{
    public readonly record struct Cell(string Header, string? Value);

    public static IReadOnlyList<Cell> Present(
        object? raw,
        ReportColumnInput column,
        EnumerationHandling handling,
        EnumerationDto? enumeration,
        string? timeZoneId)
    {
        object? value = ReportTransforms.Apply(raw, column.TransformFunction, timeZoneId);
        string header = string.IsNullOrWhiteSpace(column.Alias) ? column.Name : column.Alias;
        bool lookup = enumeration is not null && handling != EnumerationHandling.ShowNumericValue;

        if (lookup && handling == EnumerationHandling.ShowBoth)
        {
            string? text = TryWholeNumber(value, out int number) ? Lookup(enumeration!, number) : null;
            return [new Cell(header + " name", text), new Cell(header, Format(value, string.Empty))];
        }

        if (lookup && handling == EnumerationHandling.ShowText && TryWholeNumber(value, out int whole))
        {
            return [new Cell(header, Lookup(enumeration!, whole))];
        }

        return [new Cell(header, Format(value, column.Format))];
    }

    private static string? Lookup(EnumerationDto enumeration, int number)
    {
        if (!enumeration.IsBitFlags)
        {
            return enumeration.Values.FirstOrDefault(item => item.Id == number)?.Name;
        }

        var names = new StringBuilder();
        foreach (int flag in Flags(enumeration.Values, number))
        {
            string? name = enumeration.Values.FirstOrDefault(item => item.Id == flag)?.Name;
            if (name is null)
            {
                continue;
            }

            if (names.Length > 0)
            {
                names.Append(" + ");
            }

            names.Append(name);
        }

        return names.Length == 0 ? null : names.ToString();
    }

    /// <summary>The old runner walked the defined flag values and kept those set on the source.</summary>
    public static IEnumerable<int> Flags(IReadOnlyList<EnumerationValueDto> values, int source)
    {
        ulong flag = 1;
        foreach (int value in values.Select(item => item.Id))
        {
            if (value < 0)
            {
                continue;
            }

            ulong bits = (ulong)value;
            while (flag < bits)
            {
                flag <<= 1;
            }

            if (flag == bits && ((ulong)source & bits) == bits)
            {
                yield return value;
            }
        }
    }

    private static bool TryWholeNumber(object? value, out int number)
    {
        switch (value)
        {
            case byte or sbyte or short or ushort or int:
                number = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                return true;
            case long wide when wide is >= int.MinValue and <= int.MaxValue:
                number = (int)wide;
                return true;
            default:
                number = 0;
                return false;
        }
    }

    private static string? Format(object? value, string format)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(format) && value is IFormattable formattable)
        {
            return formattable.ToString(format, CultureInfo.InvariantCulture);
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }
}
