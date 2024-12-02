using System;

namespace EndlessDelivery.Common.Communication.Scores;

public class OnlineScore
{
    public ulong SteamId { get; set; }
    public Score Score { get; set; }
    public int Index { get; set; }
    public int CountryIndex { get; set; }
    public short Difficulty { get; set; }
    public DateTime Date { get; set; }
}
