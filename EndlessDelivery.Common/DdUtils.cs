using System;

namespace EndlessDelivery.Common;

public static class DdUtils
{
    public static string IntToDifficulty(int diff) => diff switch
    {
        0 => "HARMLESS",
        1 => "LENIENT",
        2 => "STANDARD",
        3 => "VIOLENT",
        4 => "BRUTAL",
        5 => "UKMD", //i think its a good idea to shorten this
        _ => throw new Exception("Invalid difficulty.")
    };

    public static string ToWordString(this TimeSpan timeSpan)
    {
        string time = $"{timeSpan.Minutes} minute{(timeSpan.Minutes == 1 ? string.Empty : "s")}";

        if (timeSpan.Hours > 0)
        {
            time = $"{timeSpan.Hours} hour{(timeSpan.Hours == 1 ? string.Empty : "s")}, " + time;
        }

        if (timeSpan.Days > 0)
        {
            time = $"{timeSpan.Days} day{(timeSpan.Days == 1 ? string.Empty : "s")}, " + time;
        }

        return time;
    }
}
