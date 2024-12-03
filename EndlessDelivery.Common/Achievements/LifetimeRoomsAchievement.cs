using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public class LifetimeRoomsAchievement : ServerSideAchievement
{
    private string _id;
    private int _rooms;

    public override string Id => _id;

    public override bool ShouldGrant(OnlineScore score, Score lifetimeStats) => lifetimeStats.Rooms >= _rooms;

    public LifetimeRoomsAchievement(string id, int rooms)
    {
        _id = id;
        _rooms = rooms;
    }
}
