using System;
using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public class MoneyAchievement : ServerSideAchievement
{
    private string _id;
    private float _money;

    public override string Id => _id;

    public override bool ShouldGrant(OnlineScore score, OnlineScore bestScore, Score lifetimeStats) => score.Score.MoneyGain > _money;

    public MoneyAchievement(string id, int money)
    {
        _id = id;
        _money = money;
    }
}
