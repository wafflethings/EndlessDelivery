using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public class BestRoomAchievement : ServerSideAchievement
{
    private string _id;
    private int _room;

    public override string Id => _id;

    public override bool ShouldGrant(OnlineScore score, OnlineScore bestScore, Score lifetimeStats) => bestScore.Score.Rooms >= _room;

    public BestRoomAchievement(string id, int room)
    {
        _id = id;
        _room = room;
    }
}
