using System;
using System.Globalization;

namespace EndlessDelivery.Utils;

public static class TimeUtils
{
    public static string Formatted(this TimeSpan span)
    {
        return span.ToString(@"mm\:ss\:fff", new CultureInfo("en-US"));
    }
}
