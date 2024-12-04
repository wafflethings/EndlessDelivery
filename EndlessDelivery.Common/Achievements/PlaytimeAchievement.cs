using System;
using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public class PlaytimeAchievement : ServerSideAchievement
{
    private string _id;
    private float _seconds;

    public override string Id => _id;

    public override bool ShouldGrant(OnlineScore score, OnlineScore bestScore, Score lifetimeStats) => lifetimeStats.Time >= _seconds;

    public PlaytimeAchievement(string id, TimeSpan time)
    {
        _id = id;
        _seconds = (float)time.TotalSeconds;
    }
}
