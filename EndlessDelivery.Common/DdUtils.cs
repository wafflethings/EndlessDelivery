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
}
