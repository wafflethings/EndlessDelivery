using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public class PositionAchievement : ServerSideAchievement
{
    private string _id;
    private int _minIndex;

    public override string Id => _id;

    public override bool ShouldGrant(OnlineScore score, Score lifetimeStats) => score.Index <= _minIndex;

    public PositionAchievement(string id, int minIndex)
    {
        _id = id;
        _minIndex = minIndex;
    }
}
