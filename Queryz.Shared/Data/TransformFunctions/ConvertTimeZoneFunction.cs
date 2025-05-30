using Queryz.Data.Entities;
using TimeZoneConverter;

namespace Queryz.Data.TransformFunctions;

public class ConvertTimeZoneFunction : ITransformFunction
{
    public string Name => "Convert Time Zone";

    public dynamic Transform(dynamic value, Report report)
    {
        if (value is not DateTime)
        {
            return value;
        }

        if (value == null)
        {
            return value;
        }

        if (report == null || string.IsNullOrEmpty(report.Group.TimeZoneId))
        {
            return value;
        }

        var timeZone = TZConvert.GetTimeZoneInfo(report.Group.TimeZoneId);

        return TimeZoneInfo.ConvertTimeFromUtc(value, timeZone);
    }
}