using System;
using System.Collections.Generic;
using System.Linq;
using TimeZoneConverter;

namespace Volo.Abp.Timing;

public static class TimeZoneHelper
{
    /// <summary>
    /// Returns timezone list ordered by display name, enriched with UTC offset, filtering out invalid ids.
    /// </summary>
    public static List<NameValue> GetTimezones(List<NameValue> timezones)
    {
        return timezones
            .OrderBy(x => x.Name)
            .Select(TryCreateNameValueWithOffset)
            .Where(x => x != null)
            .Select(x => x!)
            .ToList();
    }

    /// <summary>
    /// Builds a <see cref="NameValue"/> that includes the UTC offset in the name; returns null if the id is not found.
    /// </summary>
    public static NameValue? TryCreateNameValueWithOffset(NameValue timeZone)
    {
        try
        {
            var timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZone.Name);
            var name = $"{timeZone.Name} ({GetTimezoneOffset(timeZoneInfo)})";
            return new NameValue(name, timeZoneInfo.StandardName);
        }
        catch (TimeZoneNotFoundException)
        {
            // ignore
        }

        return null;
    }

    /// <summary>
    /// Formats the base UTC offset as "+hh:mm" or "-hh:mm" for display purposes.
    /// </summary>
    public static string GetTimezoneOffset(TimeZoneInfo timeZoneInfo)
    {
        if (timeZoneInfo.BaseUtcOffset < TimeSpan.Zero)
        {
            return "-" + timeZoneInfo.BaseUtcOffset.ToString(@"hh\:mm");
        }

        return "+" + timeZoneInfo.BaseUtcOffset.ToString(@"hh\:mm");
    }
}
